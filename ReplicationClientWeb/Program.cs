using ReplicationClientWeb.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add support for HttpClientFactory
builder.Services.AddHttpClient();

// Add replication client using the extension method
builder.Services.AddLuceneReplicationClient(options =>
{
    options.ServerUrl = "http://localhost:5000/replicate/default";
    options.IndexPath = "./Client/Index";
    options.TempPath = "./Client/Temp";
});

// Optional: console logging
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();
app.Run();
