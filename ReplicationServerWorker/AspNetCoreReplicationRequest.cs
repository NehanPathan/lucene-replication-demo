using Lucene.Net.Replicator.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

public class AspNetCoreReplicationRequest : IReplicationRequest
{
    private readonly HttpRequest _request;

    public AspNetCoreReplicationRequest(HttpRequest request)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));

        Console.WriteLine("🛰️ [AspNetCoreReplicationRequest] Constructed");
        Console.WriteLine($"📍 Path: {_request.Path}");
        var queryValues = string.Join(", ", _request.Query.Select(kv => $"{kv.Key}={kv.Value}"));
        Console.WriteLine($"🔗 Query String: {queryValues}");

        // Optional: log route values too
        var routeData = _request.HttpContext.GetRouteData();
        var routeValues = string.Join(", ", routeData.Values.Select(kv => $"{kv.Key}={kv.Value}"));
        Console.WriteLine($"🧭 Route Values: {routeValues}");

    }

    public Stream InputStream => _request.Body;

    public string Method
    {
        get
        {
            Console.WriteLine($"🔍 Method: {_request.Method}");
            return _request.Method;
        }
    }

    public string Path
    {
        get
        {
            Console.WriteLine($"🧭 Path accessed: {_request.Path}");
            return _request.Path;
        }
    }


    public string QueryParam(string name)
    {
        Console.WriteLine($"🔍 QueryParam('{name}') called");

        if (_request.Query.TryGetValue(name, out var queryVal))
        {
            Console.WriteLine($"✅ Found in query string: {name} = {queryVal}");
            return queryVal.ToString(); // Will be empty string if blank
        }


        var routeData = _request.HttpContext.GetRouteData();
        if (routeData.Values.TryGetValue(name, out var routeVal))
        {
            var val = routeVal?.ToString();
            Console.WriteLine($"✅ Found in route values: {name} = {val}");
            return val;
        }

        Console.WriteLine($"❌ {name} not found in query or route");
        return null;
    }


    public string GetHeader(string name)
    {
        var headerVal = _request.Headers[name].FirstOrDefault();
        Console.WriteLine($"📬 Header[{name}] = {headerVal}");
        return headerVal;
    }
}
