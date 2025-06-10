using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Replicator;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text.Json;

var version = LuceneVersion.LUCENE_48;
var clientName = "ClientA";
var basePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), clientName);
var indexPath = Path.Combine(basePath, "Index");
var tempPath = Path.Combine(basePath, "Temp");

if (System.IO.Directory.Exists(indexPath)) System.IO.Directory.Delete(indexPath, true);
if (System.IO.Directory.Exists(tempPath)) System.IO.Directory.Delete(tempPath, true);
System.IO.Directory.CreateDirectory(indexPath);
System.IO.Directory.CreateDirectory(tempPath);

var replicaDirectory = FSDirectory.Open(indexPath);
var handler = new IndexReplicationHandler(replicaDirectory, null);
var factory = new PerSessionDirectoryFactory(tempPath);
var replicator = new HttpReplicator("http://localhost:5000/replicate/default", new HttpClient());
var client = new ReplicationClient(replicator, handler, factory);

// Background task to poll server for updates
_ = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            client.UpdateNow();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Replication error: {ex.Message}");
        }
        await Task.Delay(10000);
    }
});

// ASP.NET Core Search API
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/search", (string q) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest("Missing query parameter: q");
    }

    var analyzer = new StandardAnalyzer(version);
    using var reader = DirectoryReader.Open(replicaDirectory);
    var searcher = new IndexSearcher(reader);
    var parser = new MultiFieldQueryParser(version, new[] { "title", "body" }, analyzer);
    var luceneQuery = parser.Parse(q);
    var hits = searcher.Search(luceneQuery, 10).ScoreDocs;

    var results = new List<object>();
    foreach (var hit in hits)
    {
        var doc = searcher.Doc(hit.Doc);
        results.Add(new
        {
            id = doc.Get("id"),
            title = doc.Get("title"),
            body = doc.Get("body")
        });
    }

    return Results.Ok(results);
});

app.MapGet("/document/{id}", (string id) =>
{
    using var reader = DirectoryReader.Open(replicaDirectory);
    for (int i = 0; i < reader.MaxDoc; i++)
    {
        var doc = reader.Document(i);
        if (doc.Get("id") == id)
        {
            return Results.Ok(new
            {
                id = doc.Get("id"),
                title = doc.Get("title"),
                body = doc.Get("body")
            });
        }
    }
    return Results.NotFound($"No document found with id {id}");
});


app.Run("http://localhost:6001");
