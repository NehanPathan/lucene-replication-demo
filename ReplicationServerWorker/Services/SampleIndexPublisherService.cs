using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using ReplicationServerWorker.Shared.Lucene;

namespace ReplicationServerWorker.Services
{
    public class SampleIndexPublisherService : BackgroundService
    {
        private readonly ILogger<SampleIndexPublisherService> _logger;
        private readonly LocalReplicator _replicator;
        private readonly IIndexWriterProvider _writerProvider;

        public SampleIndexPublisherService(
            ILogger<SampleIndexPublisherService> logger,
            IIndexWriterProvider writerProvider,
            LocalReplicator replicator)
        {
            _logger = logger;
            _writerProvider = writerProvider;
            _replicator = replicator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var writer = _writerProvider.Get("default");

            writer.UpdateDocument(new Term("id", "1"), new Document
            {
                new StringField("id", "1", Field.Store.YES),
                new TextField("title", "Hello World", Field.Store.YES),
                new TextField("body", "This is a test document.", Field.Store.YES)
            });
            writer.Commit();
            var rev = new IndexRevision(writer);
            _replicator.Publish(rev);
            _logger.LogInformation("Published Revision Version: {Version}", rev.Version);

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
    }
}
