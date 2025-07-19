using Lucene.Net.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ReplicationServerWorker.Shared.Lucene
{
    public static class LuceneDIExtensions
    {
        public static ILuceneBuilder AddLucene(this IServiceCollection services)
        {
            services.AddSingleton<IIndexReaderProvider, IndexReaderProvider>();
            services.AddSingleton<IIndexWriterProvider, IndexWriterProvider>();
            services.AddSingleton<IIndexSearcherProvider, IndexSearcherProvider>();
            services.AddSingleton<IAnalyzerProvider, AnalyzerProvider>();
            services.AddKeyedSingleton<IAnalyzerProvider>((IServiceProvider sp, object? key) =>
            {
                var keyString = key?.ToString() ?? throw new ArgumentNullException(nameof(key));
                var options = sp.GetRequiredService<IOptionsMonitor<LuceneIndexOptions>>();
                return options.Get(keyString).EffectiveAnalyzer;
            });


            return new LuceneBuilder(services);
        }
    }
}
