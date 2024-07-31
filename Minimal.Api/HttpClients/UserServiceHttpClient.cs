namespace Minimal.Api.HttpClients;
public class UserServiceHttpClient : HttpClientBase
{
    private  const string _baseUrl = "";
    public UserServiceHttpClient(HttpClient client) : base(client, new Uri(_baseUrl)) { }
}
