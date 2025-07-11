using Lucene.Net.Index;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IIndexReaderProvider
    {
        IndexReader GetShared(string name); // default
        IndexReader GetSharedIfChanged(string name); // reload if underlying index changed
    }

}
