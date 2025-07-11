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

        // IndexWriterConfig-related options
        public int? MaxBufferedDocs { get; set; }
        public double? RAMBufferSizeMB { get; set; }
        public OpenMode OpenMode { get; set; } = OpenMode.CREATE_OR_APPEND;
        public bool? UseCompoundFile { get; set; }
        public MergePolicy? MergePolicy { get; set; }

        // Effective fallbacks
        public Analyzer EffectiveAnalyzer => Analyzer ?? new StandardAnalyzer(LuceneVersion);
        public IndexDeletionPolicy EffectiveDeletionPolicy =>
            DeletionPolicy ?? new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());

        // ✅ Helper method
        public void ApplyWriterSettings(IndexWriterConfig config)
        {
            if (MaxBufferedDocs.HasValue)
                config.MaxBufferedDocs = MaxBufferedDocs.Value;

            if (RAMBufferSizeMB.HasValue)
                config.RAMBufferSizeMB = RAMBufferSizeMB.Value;

            if (UseCompoundFile.HasValue)
                config.UseCompoundFile = UseCompoundFile.Value;

            if (MergePolicy is not null)
                config.SetMergePolicy(MergePolicy);
        }

    }
}
