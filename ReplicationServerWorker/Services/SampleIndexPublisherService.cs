using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Store;
using Lucene.Net.Util;
using ReplicationServerWorker.Options;


namespace ReplicationServerWorker.Services;

public class SampleIndexPublisherService : BackgroundService
{
    private readonly ILogger<SampleIndexPublisherService> _logger;
    private readonly ReplicationServerOptions _options;
    private readonly LocalReplicator _replicator;
    private FSDirectory? _dir;

    public SampleIndexPublisherService(
        ILogger<SampleIndexPublisherService> logger,
        IOptions<ReplicationServerOptions> options, LocalReplicator replicator)
    {
        _logger = logger;
        _options = options.Value;
        _replicator = replicator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _dir = FSDirectory.Open(_options.IndexPath);
        var writer = CreateWriter(_dir);

        writer.UpdateDocument(new Term("id", "1"), new Document
        {
            new StringField("id", "1", Field.Store.YES),
            new TextField("title", "Hello World", Field.Store.YES),
            new TextField("body", "This is a test document.", Field.Store.YES)
        });
        writer.Commit();
        _replicator.Publish(new IndexRevision(writer));
        _logger.LogInformation("Published initial document");

        int counter = 2;
        while (!stoppingToken.IsCancellationRequested)
        {
            var doc = new Document
            {
                new StringField("id", counter.ToString(), Field.Store.YES),
                new TextField("title", $"Doc {counter}", Field.Store.YES),
                new TextField("body", $"This is document {counter}", Field.Store.YES)
            };

            writer.UpdateDocument(new Term("id", counter.ToString()), doc);
            writer.Commit();
            _replicator.Publish(new IndexRevision(writer));
            _logger.LogInformation("Published doc ID {Counter}", counter++);

            await Task.Delay(15000, stoppingToken);
        }
    }
    public override void Dispose()
    {
        base.Dispose();
        _dir?.Dispose();
    }

    private IndexWriter CreateWriter(FSDirectory dir)
    {
        var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
        {
            IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy())
        };
        return new IndexWriter(dir, config);
    }
}
