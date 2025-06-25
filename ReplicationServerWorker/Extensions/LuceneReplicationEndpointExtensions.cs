using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace ReplicationServerWorker.Extensions
{
    public static class LuceneReplicationEndpointExtensions
    {
        public static IEndpointRouteBuilder MapLuceneReplicationServer(
            this IEndpointRouteBuilder endpoints,
            string basePath,
            IDictionary<string, IReplicator> shardMap)
        {
            var replicationService = new ReplicationService(
                 new Dictionary<string, IReplicator>(shardMap, StringComparer.OrdinalIgnoreCase), context : "/lucene");


            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("LuceneReplication");



            var pattern = $"{basePath.TrimEnd('/')}/{{shard}}/{{action}}";

            endpoints.Map(pattern, async context =>
            {
                try
                {

                    var req = new AspNetCoreReplicationRequest(context.Request);
                    var res = new AspNetCoreReplicationResponse(context.Response);


                    var shard = req.QueryParam("shard");
                    var action = req.QueryParam("action");
                    var version = req.QueryParam("version");

                    logger.LogInformation("🧩 Debug Perform Call - Shard: {0}, Action: {1}, Version: {2}", shard, action, version);
                    logger.LogInformation("📦 Registered Shards: {0}", string.Join(", ", shardMap.Keys));

                    try
                    {
                        replicationService.Perform(req, res);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("Shard not found: {0}", req.QueryParam("shard"));
                        throw;
                    }
                    await res.FlushAsync();
                }
                catch (Exception ex)
                {
                    var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("LuceneReplication");
                    logger?.LogError(ex, "Error handling replication request.");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Replication error.");
                }
            });

            return endpoints;
        }
    }
}
