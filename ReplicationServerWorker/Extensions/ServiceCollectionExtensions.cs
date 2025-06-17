using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReplicationServerWorker.Options;
using ReplicationServerWorker.Services;

namespace ReplicationServerWorker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLuceneReplicationServer(this IServiceCollection services, Action<ReplicationServerOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddHostedService<ReplicationServerService>();
            return services;
        }
    }
}
