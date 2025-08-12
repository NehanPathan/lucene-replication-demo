using Lucene.Net.Index;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexWriterProvider : IIndexWriterProvider
    {
        private readonly IServiceProvider _sp;

        public IndexWriterProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IndexWriter Get(string name)
        {
            var registration = _sp.GetRequiredKeyedService<IndexWriterRegistration>(name);
            return registration.GetWriter(_sp);
        }
    }
}
