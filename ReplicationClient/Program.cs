using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Net.Http;
using System.Threading;

Thread.Sleep(1000); // Give server a second to start

StartReplicationClient("ClientA");
StartReplicationClient("ClientB");
StartReplicationClient("ClientC");

// Prevent app from exiting
Console.ReadLine();



void StartReplicationClient(string clientName)
{
    var basePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), clientName);
    var indexPath = System.IO.Path.Combine(basePath, "ClientIndex");
    var tempPath = System.IO.Path.Combine(basePath, "Temp");

    if (System.IO.Directory.Exists(indexPath)) System.IO.Directory.Delete(indexPath, true);
    if (System.IO.Directory.Exists(tempPath)) System.IO.Directory.Delete(tempPath, true);
    System.IO.Directory.CreateDirectory(indexPath);
    System.IO.Directory.CreateDirectory(tempPath);

    var replicaDirectory = FSDirectory.Open(indexPath);
    var handler = new IndexReplicationHandler(replicaDirectory, null);
    var factory = new PerSessionDirectoryFactory(tempPath);
    var replicator = new HttpReplicator("http://localhost:5000/replicate/default", new HttpClient());
    var client = new ReplicationClient(replicator, handler, factory);

    Task.Run(async () =>
    {
        while (true)
        {
            try
            {
                client.UpdateNow();
                using var reader = DirectoryReader.Open(replicaDirectory);
                Console.WriteLine($"🔁 [{clientName}] Found {reader.MaxDoc} docs:");
                for (int i = 0; i < reader.MaxDoc; i++)
                {
                    Console.WriteLine($"📄 [{clientName}] {reader.Document(i).Get("id")} => {reader.Document(i).Get("content")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [{clientName}] Error: {ex.Message}");
            }
            await Task.Delay(7000); // poll every 7 seconds
        }
    });
}



