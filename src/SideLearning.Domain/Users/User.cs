using SideLearning.Domain.Common;

namespace SideLearning.Domain.Users;

public sealed class User : AggregateRoot
{
    private const int MaxInterestsCount = 30;

    public UserEmail Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsSuspended { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }

    private readonly List<UserInterest> _userInterest = new();
    public IReadOnlyCollection<UserInterest> UserInterests => _userInterest.AsReadOnly();


    private User() { }

    public static User Create(Guid id, UserEmail email, string? displayName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(email);

        var user = new User
        {
            Id = id,
            Email = email,
            DisplayName = NormalizeDisplayName(displayName),
            IsActive = true,
            IsSuspended = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        user.AddDomainEvent(new UserRegisteredDomainEvent(user.Id, DateTimeOffset.UtcNow));
        return user;
    }

    public static User Rehydrate(
        Guid id,
        UserEmail email,
        string displayName,
        bool isActive,
        bool isSuspended,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        DateTimeOffset? suspendedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(email);

        return new User
        {
            Id = id,
            Email = email,
            DisplayName = NormalizeDisplayName(displayName),
            IsActive = isActive,
            IsSuspended = isSuspended,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc,
            SuspendedAtUtc = suspendedAtUtc
        };
    }

    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = NormalizeDisplayName(displayName);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserDeactivatedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserActivatedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Suspend()
    {
        if (IsSuspended)
        {
            return;
        }

        IsSuspended = true;
        SuspendedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserSuspendedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Reinstate()
    {
        if (!IsSuspended)
        {
            return;
        }

        IsSuspended = false;
        SuspendedAtUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserReinstatedDomainEvent(Id, DateTimeOffset.UtcNow));
    }

    public bool CanLogin() => IsActive && !IsSuspended;

    public void AddInterest(string label, float weight, string? context)
    {
        var interest = UserInterest.Create(label, weight, context);
        EnsureInterestLimitNotReached();
        EnsureNoDuplicateLabel(interest.Label);
        _userInterest.Add(interest);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateInterest(string currentLabel, string newLabel, float weight, string? context)
    {
        if (string.IsNullOrWhiteSpace(currentLabel))
        {
            throw new ArgumentException("Current interest label cannot be null or whitespace.", nameof(currentLabel));
        }

        var existingIndex = FindInterestIndex(currentLabel);
        if (existingIndex < 0)
        {
            throw new InvalidOperationException("Cannot update user interest that does not exist.");
        }

        var updatedInterest = UserInterest.Create(newLabel, weight, context);
        if (!string.Equals(_userInterest[existingIndex].Label, updatedInterest.Label, StringComparison.OrdinalIgnoreCase))
        {
            EnsureNoDuplicateLabel(updatedInterest.Label);
        }

        _userInterest[existingIndex] = updatedInterest;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemoveInterest(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("User interest label cannot be null or whitespace.", nameof(label));
        }

        var existingIndex = FindInterestIndex(label);
        if (existingIndex < 0)
        {
            return;
        }

        _userInterest.RemoveAt(existingIndex);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public bool HasInterest(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        return FindInterestIndex(label) >= 0;
    }

    private int FindInterestIndex(string label)
        => _userInterest.FindIndex(x => string.Equals(x.Label, label.Trim(), StringComparison.OrdinalIgnoreCase));

    private void EnsureNoDuplicateLabel(string label)
    {
        if (_userInterest.Any(x => string.Equals(x.Label, label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("User interest labels must be unique.");
        }
    }

    private void EnsureInterestLimitNotReached()
    {
        if (_userInterest.Count >= MaxInterestsCount)
        {
            throw new InvalidOperationException($"A user can have at most {MaxInterestsCount} interests.");
        }
    }

    private static string NormalizeDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        return displayName.Trim();
    }
}
