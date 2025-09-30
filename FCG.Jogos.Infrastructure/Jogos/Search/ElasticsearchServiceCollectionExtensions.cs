using FCG.Jogos.Domain.Jogos.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.Jogos.Infrastructure.Jogos.Search;

public static class ElasticsearchServiceCollectionExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Elasticsearch");
        services.Configure<ElasticsearchOptions>(opt => section.Bind(opt));
        var options = new ElasticsearchOptions();
        section.Bind(options);

        if (!options.Enable)
        {
            services.AddSingleton<IJogoSearchProvider, NoOpJogoSearchProvider>();
            return services;
        }

        services.AddHttpClient();
        services.AddSingleton<IJogoSearchProvider, ElasticJogoSearchProvider>();
        return services;
    }
}
