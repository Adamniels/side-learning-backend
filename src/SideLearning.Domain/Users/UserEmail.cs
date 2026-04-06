using System.Net.Mail;

namespace SideLearning.Domain.Users;

public sealed record UserEmail
{
    public string Value { get; }
    public string NormalizedValue { get; }

    private UserEmail(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    public static UserEmail Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var trimmed = email.Trim();
        _ = new MailAddress(trimmed);
        var normalized = trimmed.ToUpperInvariant();

        return new UserEmail(trimmed, normalized);
    }

    public override string ToString() => Value;
}
