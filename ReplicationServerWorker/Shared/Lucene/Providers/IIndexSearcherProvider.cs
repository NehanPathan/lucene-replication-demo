using Lucene.Net.Search;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IIndexSearcherProvider
    {
        IndexSearcher Get(string name);
    }
}
