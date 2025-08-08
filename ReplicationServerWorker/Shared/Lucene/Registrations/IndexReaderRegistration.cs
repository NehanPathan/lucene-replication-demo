using System;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Microsoft.Extensions.DependencyInjection;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace ReplicationServerWorker.Shared.Lucene
{
    public class IndexReaderRegistration : IDisposable
    {
        private readonly string _name;
        private readonly LuceneDirectory _directory;
        private readonly ServiceLifetime _lifetime;
        private readonly bool _enableRefreshing;
        private readonly IServiceProvider _sp;
        private DirectoryReader? _cachedReader;
        private readonly object _lock = new();
        private bool _disposed;


        public IndexReaderRegistration(string name, LuceneIndexOptions config, IServiceProvider sp)
        {
            _name = name;
            _lifetime = config.ReaderLifetime;
            _enableRefreshing = config.EnableRefreshing;
            _directory = config.DirectoryFactory?.Invoke(sp) ?? FSDirectory.Open(config.IndexPath!);
            _sp = sp;

        }

        public DirectoryReader GetReader()
        {
            return _lifetime switch
            {
                ServiceLifetime.Singleton => _enableRefreshing
                    ? GetSingletonReader()
                    : _sp.GetRequiredKeyedService<DirectoryReader>(_name),

                ServiceLifetime.Scoped or ServiceLifetime.Transient =>
                    _sp.GetRequiredKeyedService<DirectoryReader>(_name),

                _ => throw new NotSupportedException($"Unsupported lifetime: {_lifetime}")
            };
        }

        private DirectoryReader GetSingletonReader()
        {
            lock (_lock)
            {
                if (_cachedReader == null)
                {
                    _cachedReader = DirectoryReader.Open(_directory);
                }
                else
                {
                    var maybeUpdated = DirectoryReader.OpenIfChanged(_cachedReader);
                    if (maybeUpdated != null)
                    {
                        if (_cachedReader is IDisposable disposable)
                            disposable.Dispose();

                        _cachedReader = maybeUpdated;
                    }
                }

                return _cachedReader;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                _cachedReader?.Dispose();
                _cachedReader = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

    }
}
