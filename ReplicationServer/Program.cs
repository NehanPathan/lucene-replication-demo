using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Markdig;

var version = LuceneVersion.LUCENE_48;
var indexPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LuceneIndex");

// Clean and recreate index directory
if (System.IO.Directory.Exists(indexPath))
    System.IO.Directory.Delete(indexPath, true);
System.IO.Directory.CreateDirectory(indexPath);

// Initialize Lucene
var dir = FSDirectory.Open(indexPath);
var analyzer = new StandardAnalyzer(version);
var sdp = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
var config = new IndexWriterConfig(version, analyzer) { IndexDeletionPolicy = sdp };
using var writer = new IndexWriter(dir, config);

// Load issues.json
var jsonPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "issues.json");
var json = JsonNode.Parse(File.ReadAllText(jsonPath)).AsArray();

int docCounter = 1;
foreach (var issue in json)
{
    string id = issue["id"]?.ToString() ?? docCounter.ToString();
    string title = issue["title"]?.ToString() ?? "";
    string bodyMarkdown = issue["body"]?.ToString() ?? "";
    string bodyText = Markdown.ToPlainText(bodyMarkdown);

    var doc = new Document
    {
        new StringField("id", id, Field.Store.YES),
        new TextField("title", title, Field.Store.YES),
        new TextField("body", bodyText, Field.Store.YES)
    };

    writer.UpdateDocument(new Term("id", id), doc);
    docCounter++;
}
writer.Commit();

// Setup replicator
var replicator = new LocalReplicator();
replicator.Publish(new IndexRevision(writer));

_ = Task.Run(async () =>
{
    int counter = 10000;
    while (true)
    {
        await Task.Delay(10000); // every 5 seconds
        var doc = new Document
        {
            new StringField("id", counter.ToString(), Field.Store.YES),
            new TextField("title", $"Auto generated #{counter}", Field.Store.YES),
            new TextField("body", $"This is a periodic doc {counter}", Field.Store.YES)
      };
        writer.UpdateDocument(new Term("id", counter.ToString()), doc);
        writer.Commit();
        replicator.Publish(new IndexRevision(writer));
        Console.WriteLine($"✅ Published revision: doc-{counter}");
        counter++;
    }
});

// Configure ASP.NET Core
var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
var app = builder.Build();

// Replication route
var replicationService = new ReplicationService(new Dictionary<string, IReplicator>
{
    { "default", replicator }
});

app.Map("/replicate/{shard}/{action}", async (HttpContext context, string shard, string action) =>
{
    var req = new AspNetCoreReplicationRequest(context.Request);
    var res = new AspNetCoreReplicationResponse(context.Response);
    replicationService.Perform(req, res);
    await res.FlushAsync(); // Ensure response is flushed
});

app.Run("http://localhost:5000");
