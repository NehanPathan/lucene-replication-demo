using Lucene.Net.Analysis;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class AnalyzerProvider : IAnalyzerProvider
    {
        private readonly ConcurrentDictionary<string, Analyzer> _analyzers = new();
        private readonly IOptionsMonitor<LuceneIndexOptions> _options;

        public AnalyzerProvider(IOptionsMonitor<LuceneIndexOptions> options)
        {
            _options = options;
        }

        public Analyzer Get(string name)
        {
            return _analyzers.GetOrAdd(name, n => _options.Get(n).Analyzer);
        }
    }
}
