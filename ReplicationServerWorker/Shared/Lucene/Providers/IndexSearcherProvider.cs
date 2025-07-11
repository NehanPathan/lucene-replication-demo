using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexSearcherProvider : IIndexSearcherProvider
    {
        private readonly IIndexReaderProvider _readerProvider;
        private readonly ConcurrentDictionary<string, IndexSearcher> _searchers = new();

        public IndexSearcherProvider(IIndexReaderProvider readerProvider)
        {
            _readerProvider = readerProvider;
        }

        public IndexSearcher GetShared(string name)
        {
            return _searchers.GetOrAdd(name, n =>
            {
                var reader = _readerProvider.GetShared(n);
                return new IndexSearcher(reader);
            });
        }

        public IndexSearcher GetSharedIfChanged(string name)
        {
            var existing = _searchers.GetOrAdd(name, _ =>
            {
                var reader = _readerProvider.GetShared(name);
                return new IndexSearcher(reader);
            });

            var currentReader = (DirectoryReader)existing.IndexReader;
            var maybeUpdated = DirectoryReader.OpenIfChanged(currentReader);

            if (maybeUpdated != null)
            {
                var updatedSearcher = new IndexSearcher(maybeUpdated);
                _searchers[name] = updatedSearcher;
                return updatedSearcher;
            }

            return existing;
        }
    }
}
