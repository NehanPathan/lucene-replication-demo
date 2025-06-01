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

// Add sample documents
for (int i = 1; i <= 3; i++)
{
    var doc = new Document
    {
        new StringField("id", $"doc-{i}", Field.Store.YES),
        new TextField("content", $"Lucene doc {i}", Field.Store.YES)
    };
    writer.UpdateDocument(new Term("id", $"doc-{i}"), doc);
}
writer.Commit();

// Setup replicator
var replicator = new LocalReplicator();
replicator.Publish(new IndexRevision(writer));

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
