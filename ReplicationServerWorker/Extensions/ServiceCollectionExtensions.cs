using Microsoft.Extensions.DependencyInjection;
using ReplicationServerWorker.Services;
using ReplicationServerWorker.Options;

namespace ReplicationServerWorker.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReplicationServer(this IServiceCollection services, Action<ReplicationServerOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddHostedService<ReplicationServerService>();
            return services;
        }
    }
}
