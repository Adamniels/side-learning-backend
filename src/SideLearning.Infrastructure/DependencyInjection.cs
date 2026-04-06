using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Abstractions.Sessions;
using SideLearning.Application.Abstractions.Users;
using SideLearning.Infrastructure.Authentication;
using SideLearning.Infrastructure.Identity;
using SideLearning.Infrastructure.Persistence;
using SideLearning.Infrastructure.Persistence.Repositories;

namespace SideLearning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<ICredentialService, IdentityCredentialService>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        return services;
    }
}
