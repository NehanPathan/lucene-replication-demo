using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexSearcherProvider : IIndexSearcherProvider
    {
        private readonly ConcurrentDictionary<string, IndexSearcher> _searchers = new();
        private readonly IServiceProvider _sp;
        private readonly IOptionsMonitor<LuceneIndexOptions> _options;

        public IndexSearcherProvider(IServiceProvider sp, IOptionsMonitor<LuceneIndexOptions> options)
        {
            _sp = sp;
            _options = options;
        }

        public IndexSearcher Get(string name)
        {
            return _searchers.GetOrAdd(name, n =>
            {
                var config = _options.Get(n);
                var directory = config.DirectoryFactory?.Invoke(_sp) ?? FSDirectory.Open(config.IndexPath!);
                var reader = DirectoryReader.Open(directory);
                return new IndexSearcher(reader);
            });
        }
    }
}
