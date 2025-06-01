using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Net.Http;
using System.Threading;

// var version = LuceneVersion.LUCENE_48;
var slavePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ClientIndex");
var tempPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Temp");

if (System.IO.Directory.Exists(slavePath)) System.IO.Directory.Delete(slavePath, true);
if (System.IO.Directory.Exists(tempPath)) System.IO.Directory.Delete(tempPath, true);
System.IO.Directory.CreateDirectory(slavePath);
System.IO.Directory.CreateDirectory(tempPath);

var slaveDir = FSDirectory.Open(slavePath);
var handler = new IndexReplicationHandler(slaveDir, null);
var factory = new PerSessionDirectoryFactory(tempPath);

Thread.Sleep(1000); // Ensure server is up

var replicator = new HttpReplicator("http://localhost:5000/replicate/default", new HttpClient());
var client = new ReplicationClient(replicator, handler, factory);

try
{
    client.UpdateNow();
    using var reader = DirectoryReader.Open(slaveDir);
    Console.WriteLine($"📚 Found {reader.MaxDoc} docs:");
    for (int i = 0; i < reader.MaxDoc; i++)
        Console.WriteLine($"📄 {reader.Document(i).Get("id")} => {reader.Document(i).Get("content")}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}



