using Lucene.Net.Index;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IIndexReaderProvider
    {
        IndexReader Get(string name);
    }

}
