using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra;

public static class ServiceClient
{
    private static readonly IReadOnlyDictionary<ServiceName, string> __hostByServiceName = new Dictionary<ServiceName, string> {
        {
            ServiceName.Search, 
            Environment.GetEnvironmentVariable("TICKETBURST_SERVICEMAP_SEARCH") ?? "localhost:3001"
        },
        {
            ServiceName.Reservation, 
            Environment.GetEnvironmentVariable("TICKETBURST_SERVICEMAP_RESERVATION") ?? "localhost:3002"
        },  
        {
            ServiceName.Checkout, 
            Environment.GetEnvironmentVariable("TICKETBURST_SERVICEMAP_CHECKOUT") ?? "localhost:3003"
        },  
    };
    
    public static async Task<T?> HttpGetJson<T>(
        ServiceName serviceName, 
        string[]? path = null, 
        Tuple<string, string>[]? query = null)
        where T : class
    {
        var url = GetRequestUrl(serviceName, path, query);
        Console.WriteLine("HTTP GET: " + url);

        try
        {
            using var httpClient = new HttpClient();
        
            var httpResponse = await httpClient.GetAsync(url);
            var data = await LoadResponseAsJson<T>(httpResponse);
        
            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
 
    public static async Task<T?> HttpPostJson<T>(
        ServiceName serviceName, 
        string[]? path = null, 
        Tuple<string, string>[]? query = null, 
        object? body = null)
        where T : class
    {
        var url = GetRequestUrl(serviceName, path, query);
        Console.WriteLine("HTTP POST: " + url);

        try
        {
            using var httpClient = new HttpClient();

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) {
                Content = body != null 
                    ? JsonContent.Create(body)
                    : null
            };

            var httpResponse = await httpClient.SendAsync(httpRequest);
            var data = await LoadResponseAsJson<T>(httpResponse);

            return data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private static async Task<T?> LoadResponseAsJson<T>(HttpResponseMessage response)
        where T : class
    {
        Console.WriteLine($"HTTP response {(int)response.StatusCode} {response.StatusCode}");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        try
        {
             var reply = await response.Content.ReadAsAsync<ReplyContract<T>>();
             return reply?.Data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return null;
        }        
    }
    
    private static string GetRequestUrl(
        ServiceName serviceName, 
        string[]? path = null, 
        Tuple<string, string>[]? query = null)
    {
        var builder = new StringBuilder("http://");
        builder.Append(__hostByServiceName[serviceName]);

        if (path != null)
        {
            for (int i = 0; i < path.Length; i++)
            {
                builder.Append('/');
                builder.Append(path[i]);
            }
        }

        if (query != null)
        {
            for (int i = 0; i < query.Length; i++)
            {
                builder.Append(i == 0 ? '?' : '&');
                builder.Append(query[i].Item1);
                builder.Append('=');
                builder.Append(UrlEncoder.Default.Encode(query[i].Item2));
            }
        }

        return builder.ToString();
    }
}

public enum ServiceName
{
    Search,
    Reservation,
    Checkout
}
