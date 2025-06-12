namespace ReplicationServerWorker.Options
{
    public class ReplicationServerOptions
    {
        public int Port { get; set; } = 5000;
        public string IndexPath { get; set; } = @"C:\LuceneIndexes";
    }
}
