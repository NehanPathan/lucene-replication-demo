using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexWriterProvider : IIndexWriterProvider
    {
        private readonly ConcurrentDictionary<string, IndexWriter> _writers = new();
        private readonly IServiceProvider _sp;
        private readonly IOptionsMonitor<LuceneIndexOptions> _options;

        public IndexWriterProvider(IServiceProvider sp, IOptionsMonitor<LuceneIndexOptions> options)
        {
            _sp = sp;
            _options = options;
        }

        public IndexWriter Get(string name)
        {
            return _writers.GetOrAdd(name, n =>
            {
                var config = _options.Get(n);
                var directory = config.DirectoryFactory?.Invoke(_sp) ?? FSDirectory.Open(config.IndexPath!);

                // ✅ Add SnapshotDeletionPolicy
                var deletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

                var writerConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, config.Analyzer)
                {
                    IndexDeletionPolicy = deletionPolicy
                };

                return new IndexWriter(directory, writerConfig);
            });
        }
    }
}
