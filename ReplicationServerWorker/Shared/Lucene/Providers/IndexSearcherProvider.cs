using Lucene.Net.Search;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexSearcherProvider : IIndexSearcherProvider
    {
        private readonly IServiceProvider _sp;

        public IndexSearcherProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IndexSearcher Get(string name)
        {
            var registration = _sp.GetRequiredKeyedService<IndexSearcherRegistration>(name);
            return registration.GetSearcher(_sp);
        }
    }
}
