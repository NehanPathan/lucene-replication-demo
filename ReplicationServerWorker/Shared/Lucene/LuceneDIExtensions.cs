using Lucene.Net.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace ReplicationServerWorker.Shared.Lucene
{
    public static class LuceneDIExtensions
    {
        public static ILuceneBuilder AddLucene(this IServiceCollection services)
        {
            services.AddTransient<IIndexReaderProvider, IndexReaderProvider>();
            services.AddTransient<IIndexWriterProvider, IndexWriterProvider>();
            services.AddTransient<IIndexSearcherProvider, IndexSearcherProvider>();
            services.AddSingleton<IAnalyzerProvider, AnalyzerProvider>();

            return new LuceneBuilder(services);
        }

    }
}
