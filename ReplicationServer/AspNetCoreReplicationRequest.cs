using Lucene.Net.Replicator.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class AspNetCoreReplicationRequest : IReplicationRequest
{
    private readonly HttpRequest _request;

    public AspNetCoreReplicationRequest(HttpRequest request)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public Stream InputStream => _request.Body;
    
    public string Method => _request.Method;
    
    public string Path => _request.Path;
    
    // Handle both GetParameter and QueryParam methods - they should return the same thing
    public string GetParameter(string name) => _request.Query[name].FirstOrDefault();
    
    public string QueryParam(string name) => _request.Query[name].FirstOrDefault();
    
    // Handle header retrieval safely
    public string GetHeader(string name) => _request.Headers[name].FirstOrDefault();
}

