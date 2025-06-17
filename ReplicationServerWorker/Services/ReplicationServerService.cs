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

    public ReplicationServerService(
        ILogger<ReplicationServerService> logger,
        IOptions<ReplicationServerOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kestrel server on port {Port}...", _options.Port);

        var writer = CreateWriter(_options.IndexPath);
        var replicator = new LocalReplicator();

        PublishInitialDoc(writer, replicator);
        StartBackgroundPublishing(writer, replicator, stoppingToken);

        var app = SetupKestrelServer(replicator);
        app.Urls.Add($"http://localhost:{_options.Port}");
        await app.RunAsync(stoppingToken);
    }

    private IndexWriter CreateWriter(string indexPath)
    {
        var dir = FSDirectory.Open(indexPath);
        var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
        {
            IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy())
        };
        return new IndexWriter(dir, config);
    }

    private void PublishInitialDoc(IndexWriter writer, LocalReplicator replicator)
    {
        writer.UpdateDocument(new Term("id", "1"), new Document
        {
            new StringField("id", "1", Field.Store.YES),
            new TextField("title", "Hello World", Field.Store.YES),
            new TextField("body", "This is a test document.", Field.Store.YES)
        });
        writer.Commit();
        replicator.Publish(new IndexRevision(writer));
    }

    private void StartBackgroundPublishing(IndexWriter writer, LocalReplicator replicator, CancellationToken token)
    {
        _ = Task.Run(async () =>
        {
            int counter = 2;
            while (!token.IsCancellationRequested)
            {
                var doc = new Document
                {
                    new StringField("id", counter.ToString(), Field.Store.YES),
                    new TextField("title", $"Doc {counter}", Field.Store.YES),
                    new TextField("body", $"This is document {counter}", Field.Store.YES)
                };

                writer.UpdateDocument(new Term("id", counter.ToString()), doc);
                writer.Commit();
                replicator.Publish(new IndexRevision(writer));
                _logger.LogInformation("Published doc ID {Counter}", counter++);

                await Task.Delay(15000, token);
            }
        }, token);
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
