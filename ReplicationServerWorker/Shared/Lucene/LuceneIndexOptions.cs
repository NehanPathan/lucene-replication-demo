using System;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Index.Extensions;
using Lucene.Net.Store;
using Lucene.Net.Util;

using LuceneDirectory = Lucene.Net.Store.Directory;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class LuceneIndexOptions
    {
        public string? IndexPath { get; set; }
        public Func<IServiceProvider, LuceneDirectory>? DirectoryFactory { get; set; }

        public Analyzer? Analyzer { get; set; }
        public LuceneVersion LuceneVersion { get; set; } = LuceneVersion.LUCENE_48;
        public IndexDeletionPolicy? DeletionPolicy { get; set; }

        public bool EnableSearcherRefresh { get; set; } = true;
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(5);

        public Action<IServiceProvider, IndexWriterConfig>? ConfigureIndexWriterConfig { get; set; }

        // Effective fallbacks
        public Analyzer EffectiveAnalyzer => Analyzer ?? new StandardAnalyzer(LuceneVersion);
        public IndexDeletionPolicy EffectiveDeletionPolicy =>
            DeletionPolicy ?? new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

        // ✅ Helper method
        public void ApplyWriterSettings(IServiceProvider sp, IndexWriterConfig config)
        {
            ConfigureIndexWriterConfig?.Invoke(sp, config);
        }


    }
}
