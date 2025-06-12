using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReplicationClientWeb.Options;
using System.Threading;
using System.Threading.Tasks;

namespace ReplicationClientWeb.Services
{
    public class ReplicationClientService : BackgroundService
    {
        private readonly ILogger<ReplicationClientService> _logger;
        private readonly ReplicationClientOptions _options;

        public ReplicationClientService(
            ILogger<ReplicationClientService> logger,
            IOptions<ReplicationClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReplicationClientService is running.");
            _logger.LogInformation("Leader URL: {LeaderUrl}", _options.LeaderUrl);
            _logger.LogInformation("Index Path: {IndexPath}", _options.IndexPath);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Replicating from {LeaderUrl} into {IndexPath}...", _options.LeaderUrl, _options.IndexPath);
                await Task.Delay(5000, stoppingToken);
            }

            _logger.LogInformation("ReplicationClientService is stopping.");
        }
    }
}
