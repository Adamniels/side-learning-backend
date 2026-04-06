using SideLearning.Domain.Common;

namespace SideLearning.Domain.Users;

public sealed class User : AggregateRoot
{
    public UserEmail Email { get; private set; } = null!;
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsSuspended { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }

    private User()
    {
    }

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

    private static string NormalizeDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return string.Empty;
        }

        return displayName.Trim();
    }
}
