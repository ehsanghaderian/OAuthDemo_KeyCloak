namespace OrderConsumer.Services
{

    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OrderService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<OrderResponse>> GetOrdersAsync(string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/Orders");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);


            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
            Console.WriteLine($"RequestUri: {request.RequestUri}");
            Console.WriteLine(
                $"Full URI: {new Uri(_httpClient.BaseAddress!, request.RequestUri!)}");
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var orderResponses = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();

            return orderResponses;
        }
    }

    public record OrderResponse
    {
        public int Id { get; set; }
        public int Amount { get; set; }
    }
}
