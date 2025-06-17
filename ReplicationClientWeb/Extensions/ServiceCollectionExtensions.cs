using Microsoft.Extensions.DependencyInjection;
using ReplicationClientWeb.Options;
using ReplicationClientWeb.Services;
using System;

namespace ReplicationClientWeb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLuceneReplicationClient(this IServiceCollection services, Action<ReplicationClientOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddHostedService<ReplicationClientService>();
            return services;
        }
    }
}
