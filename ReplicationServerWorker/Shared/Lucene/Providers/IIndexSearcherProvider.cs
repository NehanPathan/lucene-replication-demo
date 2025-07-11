using Lucene.Net.Search;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IIndexSearcherProvider
    {
        IndexSearcher GetShared(string name); // default
        IndexSearcher GetSharedIfChanged(string name); // reload if underlying index changed
    }
}
