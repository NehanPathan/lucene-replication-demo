using System;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.DependencyInjection;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexSearcherRegistration : IDisposable
    {
        private readonly string _name;
        private readonly IServiceProvider _sp;
        private readonly ServiceLifetime _lifetime;
        private IndexSearcher? _cachedSearcher;
        private readonly object _lock = new();
        private bool _disposed;

        public IndexSearcherRegistration(string name, LuceneIndexOptions config, IServiceProvider sp)
        {
            _name = name;
            _sp = sp;
            _lifetime = config.SearcherLifetime;
        }

        public IndexSearcher GetSearcher()
        {
            return _lifetime switch
            {
                ServiceLifetime.Singleton => GetSingletonSearcher(),
                ServiceLifetime.Scoped or ServiceLifetime.Transient => _sp.GetRequiredKeyedService<IndexSearcher>(_name),
                _ => throw new NotSupportedException($"Unsupported lifetime: {_lifetime}")
            };
        }

        private IndexSearcher GetSingletonSearcher()
        {
            lock (_lock)
            {
                var readerReg = _sp.GetRequiredKeyedService<IndexReaderRegistration>(_name);
                var currentReader = readerReg.GetReader();

                if (_cachedSearcher?.IndexReader != currentReader)
                {
                    if (_cachedSearcher?.IndexReader is IDisposable disposableReader)
                        disposableReader.Dispose();

                    _cachedSearcher = new IndexSearcher(currentReader);
                }

                return _cachedSearcher;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                _cachedSearcher?.IndexReader?.Dispose();
                _cachedSearcher = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

    }
}
