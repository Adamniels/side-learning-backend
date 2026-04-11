using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SideLearning.Infrastructure.Authentication;
using Testcontainers.PostgreSql;

namespace SideLearning.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestIssuer = "SideLearning.Tests";
    private const string TestAudience = "SideLearning.Tests.Client";
    private const string TestSigningKey = "TEST_ONLY_SIGNING_KEY_32_CHARS_MINIMUM_____";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("sidelearning_it")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var inMemory = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:SigningKey"] = TestSigningKey,
                ["Jwt:AccessTokenMinutes"] = "30",
                ["Jwt:RefreshTokenDays"] = "7",
                ["SessionDesigner:EnableWorker"] = "false",
                ["SessionDesigner:BaseUrl"] = "http://127.0.0.1:59999",
                ["SessionDesigner:SharedSecret"] = "test_session_designer_secret",
                ["PublicApi:BaseUrl"] = "http://localhost"
            };

            config.AddInMemoryCollection(inMemory);
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<JwtOptions>(options =>
            {
                options.Issuer = TestIssuer;
                options.Audience = TestAudience;
                options.SigningKey = TestSigningKey;
                options.AccessTokenMinutes = 30;
                options.RefreshTokenDays = 7;
            });

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = TestIssuer,
                    ValidAudience = TestAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
