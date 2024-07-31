namespace Minimal.Api.HttpClients;
public class HttpClientBase
{
    protected HttpClient _httpClient { get; set; }
    public HttpClientBase(HttpClient client, Uri uri)
    {
        _httpClient = client;
        _httpClient.BaseAddress = uri;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProductsService");
    }
}
