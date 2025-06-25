using ReplicationServerWorker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplicationServerWorker.Extensions;
using Lucene.Net.Replicator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLuceneReplicationServer(options =>
{
    options.Port = 5000;
    options.IndexPath = "./Server/Index";
},useBackgroundService: false); // Do NOT launch Kestrel

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information); 
});

var app = builder.Build();


// Access the replicator from DI
var replicator = app.Services.GetRequiredService<LocalReplicator>();


// Map Lucene replication under "/lucene" base path
Console.WriteLine("Calling MapLuceneReplicationServer...");

var shardMap = new Dictionary<string, IReplicator>(StringComparer.OrdinalIgnoreCase)
{
    { "default", replicator }
};
app.MapLuceneReplicationServer("/lucene", shardMap);

app.Run();
