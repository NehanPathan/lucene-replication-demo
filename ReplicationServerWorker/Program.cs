using ReplicationServerWorker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReplicationServerWorker.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddLuceneReplicationServer(options =>
        {
            options.Port = 5000;
            options.IndexPath = "./Server/Index";
        });

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
    })
    .Build();

host.Run();
