using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SideLearning.Application.Features.Users.Interests.Add;
using SideLearning.Application.Features.Users.Interests.List;
using SideLearning.Application.Features.Users.Interests.Remove;
using SideLearning.Application.Features.Users.Interests.Update;

namespace SideLearning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<Features.Auth.Register.RegisterCommandHandler>();
        services.AddScoped<Features.Auth.Login.LoginCommandHandler>();
        services.AddScoped<Features.Auth.Refresh.RefreshTokenCommandHandler>();
        services.AddScoped<Features.Auth.Revoke.RevokeRefreshCommandHandler>();
        services.AddScoped<ListUserInterestsQueryHandler>();
        services.AddScoped<AddUserInterestCommandHandler>();
        services.AddScoped<UpdateUserInterestCommandHandler>();
        services.AddScoped<RemoveUserInterestCommandHandler>();
        return services;
    }
}
