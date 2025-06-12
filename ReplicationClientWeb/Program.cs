using ReplicationClientWeb.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add replication client using your extension method
builder.Services.AddReplicationClient(options =>
{
    options.LeaderUrl = "http://localhost:5000";
    options.IndexPath = "C:\\LuceneIndexes";
});

// Optional logging
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();

app.MapGet("/", () => "Replication Client Running!");

app.Run();
