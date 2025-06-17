using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReplicationClientWeb.Options;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Store;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Index;
using System.Net.Http;

namespace ReplicationClientWeb.Services
{
    public class ReplicationClientService : BackgroundService
    {
        private readonly ILogger<ReplicationClientService> _logger;
        private readonly ReplicationClientOptions _options;
        private readonly HttpClient _httpClient;

        public ReplicationClientService(
            ILogger<ReplicationClientService> logger,
            IOptions<ReplicationClientOptions> options,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _options = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var replicaDirectory = FSDirectory.Open(_options.IndexPath);
            var handler = new IndexReplicationHandler(replicaDirectory, null);
            var factory = new PerSessionDirectoryFactory(_options.TempPath);
            var replicator = new HttpReplicator(_options.ServerUrl, _httpClient);
            var client = new ReplicationClient(replicator, handler, factory);

            _logger.LogInformation("Client pulling from {ServerUrl}", _options.ServerUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    client.UpdateNow();
                    _logger.LogInformation("Replication successful.");

                    try
                    {
                        using var reader = DirectoryReader.Open(replicaDirectory);
                        _logger.LogInformation("Client index now has {DocCount} docs", reader.NumDocs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Unable to open index reader (possibly not ready yet).");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Replication failed");
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
