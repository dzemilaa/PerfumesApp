using orderservice.Models;

namespace orderservice
{
    public interface IShopServiceClient
    {
        Task<List<CartItemDto>> GetCartItems(Guid userId);
        Task<bool> ClearCart(Guid userId);
    }
}
