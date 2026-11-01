using HotChocolate;
using Microsoft.EntityFrameworkCore;
using productservice.Data;
using productservice.Models;

namespace productservice.GraphQL
{
    public class Mutation
    {
        public async Task<ProductResult> AddProduct(
            [GraphQLNonNullType] string name,
            string? description,
            decimal price,
            int discount,
            int stock,
            int brandId,
            int genderId,
            int size,
            string? imageUrl,
            [Service] ProductsDbContext db)
        {
            var product = new products
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description ?? "",
                Price = price,
                Discount = discount,
                Stock = stock,
                BrandId = brandId,
                GenderId = genderId,
                Size = size,
                ImageUrl = imageUrl ?? ""
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return new ProductResult(true, "Product added successfully.", MapToView(product));
        }

        public async Task<ProductResult> UpdateProduct(
            [GraphQLNonNullType] string id,
            string? name,
            string? description,
            decimal? price,
            int? discount,
            int? stock,
            int? brandId,
            int? genderId,
            int? size,
            string? imageUrl,
            [Service] ProductsDbContext db)
        {
            if (!Guid.TryParse(id, out var productGuid))
                return new ProductResult(false, "Invalid product ID.", null);

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productGuid);
            if (product == null)
                return new ProductResult(false, "Product not found.", null);

            if (name != null) product.Name = name;
            if (description != null) product.Description = description;
            if (price.HasValue) product.Price = price.Value;
            if (discount.HasValue) product.Discount = discount.Value;
            if (stock.HasValue) product.Stock = stock.Value;
            if (brandId.HasValue) product.BrandId = brandId.Value;
            if (genderId.HasValue) product.GenderId = genderId.Value;
            if (size.HasValue) product.Size = size.Value;
            if (imageUrl != null) product.ImageUrl = imageUrl;

            await db.SaveChangesAsync();

            return new ProductResult(true, "Product updated successfully.", MapToView(product));
        }

        public async Task<ProductResult> DeleteProduct(
            [GraphQLNonNullType] string id,
            [Service] ProductsDbContext db)
        {
            if (!Guid.TryParse(id, out var productGuid))
                return new ProductResult(false, "Invalid product ID.", null);

            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productGuid);
            if (product == null)
                return new ProductResult(false, "Product not found.", null);

            db.Products.Remove(product);
            await db.SaveChangesAsync();

            return new ProductResult(true, "Product deleted successfully.", null);
        }

        private ProductView MapToView(products p) => new ProductView(
            p.Id, p.Name, p.Description, p.Price,
            p.Discount, p.Stock,
            p.BrandId, p.GenderId, p.Size, p.ImageUrl
        );

        public async Task<BrandResult> AddBrand(
    [GraphQLNonNullType] string name,
    [Service] ProductsDbContext db)
        {
            var existing = await db.Brands.FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
            if (existing != null)
                return new BrandResult(false, "Brand already exists.", existing);

            var brand = new Brands { Name = name };
            db.Brands.Add(brand);
            await db.SaveChangesAsync();

            return new BrandResult(true, "Brand added successfully.", brand);
        }

        public class BrandResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public Brands? Brand { get; set; }

            public BrandResult() { }
            public BrandResult(bool success, string message, Brands? brand)
            {
                Success = success;
                Message = message;
                Brand = brand;
            }
        }
    }

    public class ProductResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProductView? Product { get; set; }

        public ProductResult() { }
        public ProductResult(bool success, string message, ProductView? product)
        {
            Success = success;
            Message = message;
            Product = product;
        }
    }

    public class ProductView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int Stock { get; set; }
        public int BrandId { get; set; }
        public int GenderId { get; set; }
        public int Size { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public ProductView(Guid id, string name, string description, decimal price,
            int discount, int stock, int brandId, int genderId, int size, string imageUrl)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Discount = discount;
            Stock = stock;
            BrandId = brandId;
            GenderId = genderId;
            Size = size;
            ImageUrl = imageUrl;
        }
    }
}