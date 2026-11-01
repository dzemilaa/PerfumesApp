using Microsoft.EntityFrameworkCore;
using ShopService.Data;
using shopservice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Mutation
{
    private readonly IDbContextFactory<ShopDbContext> _contextFactory;
    private readonly IProductServiceClient _productService;

    public Mutation(IDbContextFactory<ShopDbContext> contextFactory, IProductServiceClient productService)
    {
        _contextFactory = contextFactory;
        _productService = productService;
    }

    public async Task<CartResult> AddToCart(string userId, string productId, int quantity)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        if (!Guid.TryParse(userId, out var userIdGuid))
            return new CartResult(false, "Invalid userId.", new());

        if (!Guid.TryParse(productId, out var productIdGuid))
            return new CartResult(false, "Invalid productId.", new());

        if (quantity <= 0)
            return new CartResult(false, "Quantity must be greater than zero.", new());

        var cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userIdGuid);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userIdGuid,
                Items = new List<CartItem>()
            };
            dbContext.Carts.Add(cart);
            await dbContext.SaveChangesAsync();
        }
        var affected = await dbContext.CartItems
            .Where(ci => ci.CartId == cart.Id && ci.ProductId == productIdGuid)
            .ExecuteUpdateAsync(s => s.SetProperty(ci => ci.Quantity, ci => ci.Quantity + quantity));
        if (affected == 0)
        {
            dbContext.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = productIdGuid,
                Quantity = quantity
            });
            await dbContext.SaveChangesAsync();
        }

        await dbContext.Entry(cart).Collection(c => c.Items).LoadAsync();

        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var productsDict = await _productService.GetProductsByIdsAsync(productIds);

        var items = cart.Items.Select(ci =>
        {
            if (productsDict.TryGetValue(ci.ProductId, out var p))
                return new CartItemView(ci.ProductId, p.Name, p.Price, ci.Quantity, p.Image);
            return new CartItemView(ci.ProductId, null, 0, ci.Quantity, null);
        }).ToList();

        return new CartResult(true, "Cart updated.", items);
    }

    public async Task<bool> ClearCart([GraphQLNonNullType] Guid userId, [Service] ShopDbContext dbContext)
    {
        var cartItems = await dbContext.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return true; 

        dbContext.CartItems.RemoveRange(cartItems);
        await dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<CartResult> RemoveFromCart(string userId, string productId)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        if (!Guid.TryParse(userId, out var userIdGuid))
            return new CartResult(false, "Invalid userId.", new());

        if (!Guid.TryParse(productId, out var productIdGuid))
            return new CartResult(false, "Invalid productId.", new());

        var cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userIdGuid);

        if (cart == null)
            return new CartResult(false, "Cart not found.", new());

        var decremented = await dbContext.CartItems
            .Where(ci => ci.CartId == cart.Id && ci.ProductId == productIdGuid && ci.Quantity > 1)
            .ExecuteUpdateAsync(s => s.SetProperty(ci => ci.Quantity, ci => ci.Quantity - 1));

        if (decremented == 0)
        {
            await dbContext.CartItems
                .Where(ci => ci.CartId == cart.Id && ci.ProductId == productIdGuid)
                .ExecuteDeleteAsync();
        }

        await dbContext.Entry(cart).Collection(c => c.Items).LoadAsync();

        var productIds = cart.Items.Select(i => i.ProductId).ToList();
        var productsDict = await _productService.GetProductsByIdsAsync(productIds);

        var items = cart.Items.Select(ci =>
        {
            if (productsDict.TryGetValue(ci.ProductId, out var p))
                return new CartItemView(ci.ProductId, p.Name, p.Price, ci.Quantity, p.Image);
            return new CartItemView(ci.ProductId, null, 0, ci.Quantity, null);
        }).ToList();

        return new CartResult(true, "Cart updated.", items);
    }

   
    public async Task<OperationResult> AddToFavorite(string userId, string productId)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        if (!Guid.TryParse(userId, out var userIdGuid))
            return new OperationResult(false, "Invalid userId.");

        if (!Guid.TryParse(productId, out var productIdGuid))
            return new OperationResult(false, "Invalid productId.");

        var favorite = await dbContext.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userIdGuid);

        if (favorite == null)
        {
            favorite = new Favorite
            {
                Id = Guid.NewGuid(),
                UserId = userIdGuid
            };
            dbContext.Favorites.Add(favorite);
            await dbContext.SaveChangesAsync(); 
        }
        var exists = await dbContext.FavoriteItems
            .AnyAsync(fi => fi.FavoriteId == favorite.Id && fi.ProductId == productIdGuid);

        if (exists)
            return new OperationResult(true, "Product already in favorite list.");

        var favoriteItem = new FavoriteItem
        {
            Id = Guid.NewGuid(),
            FavoriteId = favorite.Id,
            ProductId = productIdGuid
        };
        dbContext.FavoriteItems.Add(favoriteItem);
        await dbContext.SaveChangesAsync();

        return new OperationResult(true, "Product added to favorite list.");
    }

    public async Task<OperationResult> RemoveFromFavorite(string userId, string productId)
    {
        using var dbContext = _contextFactory.CreateDbContext();

        if (!Guid.TryParse(userId, out var userIdGuid))
            return new OperationResult(false, "Invalid userId.");

        if (!Guid.TryParse(productId, out var productIdGuid))
            return new OperationResult(false, "Invalid productId.");

        var item = await dbContext.FavoriteItems
            .Include(i => i.Favorite)
            .FirstOrDefaultAsync(i => i.Favorite.UserId == userIdGuid && i.ProductId == productIdGuid);

        if (item == null)
            return new OperationResult(false, "Product not in favorite list.");

        dbContext.FavoriteItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return new OperationResult(true, "Product removed from favorite list.");
    }
}


public class CartResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<CartItemView> Items { get; set; } = new List<CartItemView>();

    public CartResult() { }
    public CartResult(bool success, string message, List<CartItemView> items)
    {
        Success = success;
        Message = message;
        Items = items;
    }
}

public class CartItemView
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? Image { get; set; }

    public CartItemView() { }
    public CartItemView(Guid productId, string? name, decimal price, int quantity, string? image)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
        Image = image;
    }
}

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public OperationResult() { }
    public OperationResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}