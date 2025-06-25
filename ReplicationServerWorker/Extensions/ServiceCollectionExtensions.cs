using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Lucene.Net.Replicator;
using ReplicationServerWorker.Options;
using ReplicationServerWorker.Services;

namespace ReplicationServerWorker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLuceneReplicationServer(this IServiceCollection services, Action<ReplicationServerOptions> configureOptions, bool useBackgroundService = true)
        {
            services.Configure(configureOptions);
            services.AddSingleton<LocalReplicator>();

            services.AddHostedService<SampleIndexPublisherService>();

            if (useBackgroundService)
            {
                services.AddHostedService<ReplicationServerService>();
            }


            return services;
        }
    }
}
