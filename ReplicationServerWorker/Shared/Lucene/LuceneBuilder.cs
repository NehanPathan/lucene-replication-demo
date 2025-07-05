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
            _services.Configure<LuceneIndexOptions>(name, configure);
            return this;
        }
    }
}
