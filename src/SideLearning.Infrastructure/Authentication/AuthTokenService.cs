using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Infrastructure.Identity;
using SideLearning.Infrastructure.Persistence;

namespace SideLearning.Infrastructure.Authentication;

public sealed class AuthTokenService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> jwtOptions) : IAuthTokenService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public async Task<AuthTokenPair> IssueForPrincipalAsync(AuthTokenPrincipal principal, CancellationToken cancellationToken)
    {
        var access = CreateAccessToken(principal.UserId, principal.Email, principal.Roles);
        var refresh = await CreateRefreshTokenAsync(principal.UserId, cancellationToken);
        return new AuthTokenPair(access.Token, refresh.RawToken, access.ExpiresAtUtc, refresh.ExpiresAtUtc);
    }

    public async Task<AuthTokenPair?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = HashToken(refreshToken);
        var existing = await dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var user = existing.User;
        var roles = await userManager.GetRolesAsync(user);
        var newRaw = GenerateOpaqueToken();
        var newHash = HashToken(newRaw);

        existing.RevokedAtUtc = DateTimeOffset.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        var newEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays)
        };

        dbContext.RefreshTokens.Add(newEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var access = CreateAccessToken(user.Id, user.Email ?? string.Empty, roles.ToList());
        return new AuthTokenPair(access.Token, newRaw, access.ExpiresAtUtc, newEntity.ExpiresAtUtc);
    }

    public async Task RevokeRefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = HashToken(refreshToken);
        var existing = await dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);
        if (existing is null || existing.RevokedAtUtc is not null)
        {
            return;
        }

        existing.RevokedAtUtc = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(string RawToken, DateTimeOffset ExpiresAtUtc)> CreateRefreshTokenAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var raw = GenerateOpaqueToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(raw),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays)
        };

        dbContext.RefreshTokens.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (raw, entity.ExpiresAtUtc);
    }

    private (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(Guid userId, string email, IReadOnlyList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(token), expires);
    }

    private static string GenerateOpaqueToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
