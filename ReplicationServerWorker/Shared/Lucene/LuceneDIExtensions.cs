using Microsoft.Extensions.DependencyInjection;

namespace ReplicationServerWorker.Shared.Lucene
{
    public static class LuceneDIExtensions
    {
        public static ILuceneBuilder AddLucene(this IServiceCollection services)
        {
            services.AddSingleton<IIndexWriterProvider, IndexWriterProvider>();
            services.AddSingleton<IIndexSearcherProvider, IndexSearcherProvider>();
            services.AddSingleton<IAnalyzerProvider, AnalyzerProvider>();

            return new LuceneBuilder(services);
        }
    }
}
