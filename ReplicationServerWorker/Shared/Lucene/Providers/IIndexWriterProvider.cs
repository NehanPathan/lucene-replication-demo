using Lucene.Net.Index;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IIndexWriterProvider
    {
        IndexWriter Get(string name);
    }
}
