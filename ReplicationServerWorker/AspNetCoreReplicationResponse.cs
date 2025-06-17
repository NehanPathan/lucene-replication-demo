using Lucene.Net.Replicator.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
public class AspNetCoreReplicationResponse : IReplicationResponse
{
    private readonly HttpResponse _response;

    public AspNetCoreReplicationResponse(HttpResponse response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    public Stream OutputStream => _response.Body;
    
    public Stream Body => _response.Body;
    
    public int StatusCode 
    { 
        get => _response.StatusCode;
        set => _response.StatusCode = value;
    }
    
    public void SetStatusCode(int code) 
    {
        Console.WriteLine($"🔧 Response: Setting status code to {code}");
        _response.StatusCode = code;
    }
    
    public void SetHeader(string name, string value) 
    {
        if (!string.IsNullOrEmpty(name) && value != null)
        {
            Console.WriteLine($"🔧 Response: Setting header {name} = {value}");
            _response.Headers[name] = value;
        }
    }

    public void Flush() 
    {
        // ASP.NET Core doesn't allow synchronous operations by default
        // We need to use async version and wait for it
        if (_response.Body.CanWrite)
        {
            try
            {
                _response.Body.FlushAsync().GetAwaiter().GetResult();
            }
            catch (InvalidOperationException)
            {
                // If async flush fails, try to enable sync operations temporarily
                // or just skip the flush as it's often not critical
                try
                {
                    _response.Body.Flush();
                }
                catch
                {
                    // Ignore flush errors as they're typically not critical
                }
            }
        }
    }
    

    // Additional method that might be useful for async scenarios
    public async Task FlushAsync()
    {
        if (_response.Body.CanWrite)
        {
            await _response.Body.FlushAsync();
        }
    }
}