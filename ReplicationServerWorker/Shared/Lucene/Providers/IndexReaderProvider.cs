using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexReaderProvider : IIndexReaderProvider
    {
        private readonly ConcurrentDictionary<string, IndexReader> _readers = new();
        private readonly IServiceProvider _sp;
        private readonly IOptionsMonitor<LuceneIndexOptions> _options;

        public IndexReaderProvider(IServiceProvider sp, IOptionsMonitor<LuceneIndexOptions> options)
        {
            _sp = sp;
            _options = options;
        }

        public IndexReader GetShared(string name)
        {
            return _readers.GetOrAdd(name, n =>
            {
                var config = _options.Get(n);
                var directory = config.DirectoryFactory?.Invoke(_sp) ?? FSDirectory.Open(config.IndexPath!);
                return DirectoryReader.Open(directory);
            });
        }

        public IndexReader GetSharedIfChanged(string name)
        {
            var current = GetShared(name);
            return DirectoryReader.OpenIfChanged((DirectoryReader)current) ?? current;
        }
    }

}