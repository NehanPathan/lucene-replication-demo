using System;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface ILuceneBuilder
    {
        ILuceneBuilder AddIndex(string name, Action<LuceneIndexOptions> configure);
    }
}
