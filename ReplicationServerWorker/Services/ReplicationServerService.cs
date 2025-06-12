using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReplicationServerWorker.Options;

namespace ReplicationServerWorker.Services
{
    public class ReplicationServerService : BackgroundService
    {
        private readonly ILogger<ReplicationServerService> _logger;
        private readonly ReplicationServerOptions _options;

        public ReplicationServerService(
            ILogger<ReplicationServerService> logger,
            IOptions<ReplicationServerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReplicationServerService is running.");
            _logger.LogInformation("Listening on port: {Port}", _options.Port);
            _logger.LogInformation("Serving from index path: {IndexPath}", _options.IndexPath);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Replication server is alive and handling requests...");
                await Task.Delay(5000, stoppingToken); // Simulate server activity
            }

            _logger.LogInformation("ReplicationServerService is stopping.");
        }
    }
}
