namespace orderservice
{
    using orderservice.Models;
    using System.Text;
    using System.Text.Json;

    public class ShopServiceClient : IShopServiceClient
    {
        private readonly HttpClient _httpClient;

        public ShopServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ClearCart(Guid userId)
        {
            var mutation = @"
    mutation ClearCart($userId: UUID!) {
        clearCart(userId: $userId)
    }";

            var requestBody = new
            {
                query = mutation,
                variables = new { userId } 
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("graphql", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"GraphQL error: {response.StatusCode}, {json}");

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("data")
                .GetProperty("clearCart")
                .GetBoolean();
        }

        public async Task<List<CartItemDto>> GetCartItems(Guid userId)
        {
            var query = @"
            query($userId: String!) {
                cart(userId: $userId) {
                    productId
                    name
                    price
                    quantity
                    image
                }
            }";

            var requestBody = new
            {
                query,
                variables = new { userId = userId.ToString() }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var cartElement = doc.RootElement
                .GetProperty("data")
                .GetProperty("cart");

            return JsonSerializer.Deserialize<List<CartItemDto>>(
                cartElement.GetRawText(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<CartItemDto>();
        }
    }
}