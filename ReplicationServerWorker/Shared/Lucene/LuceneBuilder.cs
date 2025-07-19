using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class LuceneBuilder : ILuceneBuilder
    {
        private readonly IServiceCollection _services;

        public LuceneBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public ILuceneBuilder AddIndex(string name, Action<LuceneIndexOptions> configure)
        {
            _services.Configure<LuceneIndexOptions>(name, options =>
            {
                configure(options);
                var existingConfig = options.ConfigureIndexWriterConfig;

                options.ConfigureIndexWriterConfig = (sp, config) =>
                {
                    existingConfig?.Invoke(sp, config);

                    // Now Can Add our own default logic here below 
                    if(config.MaxBufferedDocs <= 0)
                    {
                        config.MaxBufferedDocs = 1000;
                    }
                };
            });
            return this;
        }
    }
}
