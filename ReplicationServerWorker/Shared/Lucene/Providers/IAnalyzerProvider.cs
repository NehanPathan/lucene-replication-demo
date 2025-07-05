using Lucene.Net.Analysis;

namespace ReplicationServerWorker.Shared.Lucene
{
    public interface IAnalyzerProvider
    {
        Analyzer Get(string name);
    }
}
