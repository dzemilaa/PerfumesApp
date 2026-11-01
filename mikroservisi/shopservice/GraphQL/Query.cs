using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShopService.Data;
using HotChocolate;

namespace ShopService.GraphQL;

public class Query
{
    private readonly IProductServiceClient _productClient;

    public Query(IProductServiceClient productClient)
    {
        _productClient = productClient;
    }

    public async Task<IReadOnlyList<CartItemView>> Cart(
        string userId,
        [Service] ShopDbContext dbContext)
    {
        if (!Guid.TryParse(userId, out var userIdGuid))
            throw new ArgumentException("Invalid userId format.");

        var cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userIdGuid);

        if (cart == null || cart.Items.Count == 0)
            return Array.Empty<CartItemView>();

        var productIds = cart.Items.Select(i => i.ProductId).ToList();

        Dictionary<Guid, ProductDto> productDict;
        try
        {
            productDict = await _productClient.GetProductsByIdsAsync(productIds);
        }//podaci o proizvodu preko id-a
        catch
        {
            // prazna lista ako servis ne radi
            productDict = new Dictionary<Guid, ProductDto>();
        }

        return cart.Items.Select(item =>
        {
            productDict.TryGetValue(item.ProductId, out var product);

            return new CartItemView(
                item.ProductId,
                Name: product?.Name,
                Price: product?.Price ?? 0,
                item.Quantity,
                Image: product?.Image
            );
        }).ToList();
    }

    public async Task<IReadOnlyList<FavoriteItemView>> Favorite(
        string userId,
        [Service] ShopDbContext dbContext)
    {
        if (!Guid.TryParse(userId, out var userIdGuid))
            throw new ArgumentException("Invalid userId format.");

        var favorite = await dbContext.Favorites
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.UserId == userIdGuid);

        if (favorite == null || favorite.Items.Count == 0)
            return Array.Empty<FavoriteItemView>();

        var productIds = favorite.Items.Select(i => i.ProductId).ToList();

        Dictionary<Guid, ProductDto> productDict;
        try
        {
            productDict = await _productClient.GetProductsByIdsAsync(productIds);
        }
        catch
        {
            productDict = new Dictionary<Guid, ProductDto>();
        }

        return favorite.Items.Select(item =>
        {
            productDict.TryGetValue(item.ProductId, out var product);

            return new FavoriteItemView(
                item.ProductId,
                Name: product?.Name,
                Price: product?.Price ?? 0,
                Image: product?.Image
            );
        }).ToList();
    }
}

[GraphQLName("CartItem")]
public record CartItemView(
    Guid ProductId,
    string? Name,
    decimal Price,
    int Quantity,
    string? Image
);

[GraphQLName("FavoriteItem")]
public record FavoriteItemView(
    Guid ProductId,
    string? Name,
    decimal Price,
    string? Image
);