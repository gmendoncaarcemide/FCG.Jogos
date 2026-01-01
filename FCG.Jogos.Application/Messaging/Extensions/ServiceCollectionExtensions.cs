using FCG.Jogos.Application.Messaging.Configuration;
using FCG.Jogos.Application.Messaging.Interfaces;
using FCG.Jogos.Application.Messaging.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Jogos.Application.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMQSettings>(options => 
            configuration.GetSection("RabbitMQ").Bind(options));
        
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        return services;
    }
}
