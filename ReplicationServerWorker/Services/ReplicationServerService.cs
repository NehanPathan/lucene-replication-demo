using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using ReplicationServerWorker.Options;

namespace ReplicationServerWorker.Services;

public class ReplicationServerService : BackgroundService
{
    private readonly ILogger<ReplicationServerService> _logger;
    private readonly ReplicationServerOptions _options;

    public static LocalReplicator _replicator;

    public ReplicationServerService(
        ILogger<ReplicationServerService> logger,
        IOptions<ReplicationServerOptions> options, LocalReplicator replicator)
    {
        _logger = logger;
        _options = options.Value;
        _replicator = replicator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kestrel server on port {Port}...", _options.Port);

        var app = SetupKestrelServer(_replicator);
        app.Urls.Add($"http://localhost:{_options.Port}");
        await app.RunAsync(stoppingToken);
    }
    private WebApplication SetupKestrelServer(LocalReplicator replicator)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
        var app = builder.Build();

        var service = new ReplicationService(new Dictionary<string, IReplicator>
        {
            { "default", replicator }
        });

        app.Map("/replicate/{shard}/{action}", async (HttpContext context, string shard, string action) =>
        {
            try
            {
                var req = new AspNetCoreReplicationRequest(context.Request);
                var res = new AspNetCoreReplicationResponse(context.Response);
                service.Perform(req, res);
                await res.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling replication request.");
            }
        });

        return app;
    }
}
