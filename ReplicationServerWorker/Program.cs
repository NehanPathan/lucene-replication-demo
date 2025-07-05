using ReplicationServerWorker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplicationServerWorker.Extensions;
using Lucene.Net.Replicator;
using ReplicationServerWorker.Shared.Lucene;

var builder = WebApplication.CreateBuilder(args);

// Allow sync IO for Lucene's replication stream handling
builder.WebHost.ConfigureKestrel(options =>
{
    options.AllowSynchronousIO = true;
});

// Register Lucene replication services without Kestrel background service
builder.Services.AddLuceneReplicationServer(options =>
{
    options.Port = 5000;
    options.IndexPath = "./Server/Index";
}, useBackgroundService: false);

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddLucene()
    .AddIndex("default", options =>
    {
        options.IndexPath = "./Indexes/Default";
    });


var app = builder.Build();

// Get replicator instance
var replicator = app.Services.GetRequiredService<LocalReplicator>();

// Register endpoint for Lucene replication
var shardMap = new Dictionary<string, IReplicator>(StringComparer.OrdinalIgnoreCase)
{
    { "default", replicator }
};
app.MapLuceneReplicationServer("/lucene", shardMap);

app.Run();
