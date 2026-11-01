using System.Net.Http.Json;
using System.Text.Json;

namespace reviewservice.Services
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _http;

        public OrderServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> ValidateOrderAndProduct(Guid userId, Guid orderId, Guid productId)
        {
            var query = new
            {
                query = $@"query {{
                    getOrders(userId: ""{userId}"") {{
                        id
                        orderItems {{
                            productId
                        }}
                    }}
                }}"
            };

            var response = await _http.PostAsJsonAsync("", query);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return false;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<GetOrdersResponse>(raw, options);
            var orders = result?.Data?.GetOrders;


            if (orders == null) return false;

            var order = orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null) return false;

            var productFound = order.OrderItems?.Any(i => i.ProductId == productId) ?? false;

            return productFound;
        }
    }

    public class GetOrdersResponse
    {
        public GetOrdersData? Data { get; set; }
    }

    public class GetOrdersData
    {
        public List<OrderViewMin>? GetOrders { get; set; }
    }

    public class OrderViewMin
    {
        public Guid Id { get; set; }
        public List<OrderItemMin>? OrderItems { get; set; }
    }

    public class OrderItemMin
    {
        public Guid ProductId { get; set; }
    }
}