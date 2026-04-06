using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<Features.Topics.CreateTopic.CreateTopicCommandHandler>();
        services.AddScoped<Features.Topics.GetTopics.GetTopicsQueryHandler>();
        services.AddScoped<Features.Topics.GetTopicById.GetTopicByIdQueryHandler>();
        return services;
    }
}
