using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using Lucene.Net.Util;

using LuceneDirectory = Lucene.Net.Store.Directory;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class LuceneIndexOptions
    {
        public string? IndexPath { get; set; }
        public Func<IServiceProvider, LuceneDirectory>? DirectoryFactory { get; set; }
        public Analyzer Analyzer { get; set; } = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        public bool EnableSearcherRefresh { get; set; } = true;
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
