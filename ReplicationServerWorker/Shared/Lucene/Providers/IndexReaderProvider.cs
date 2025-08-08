using Lucene.Net.Index;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexReaderProvider : IIndexReaderProvider
    {
        private readonly IServiceProvider _sp;

        public IndexReaderProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IndexReader Get(string name)
        {
            var registration = _sp.GetRequiredKeyedService<IndexReaderRegistration>(name);
            return registration.GetReader();
        }
    }
}
