using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using productservice.Data;
using productservice.Models;
using System.Linq;

namespace productservice.GraphQL
{
    public class Query
    {
        [UsePaging]
        [UseProjection]//moguc odabir odredjenih podataka
        [UseFiltering]//mozemo da koristimo where 
        [UseSorting]
        public IQueryable<products> GetProducts([Service] ProductsDbContext db, string? name = null, string? brandName = null, int? size = null)
        {
            IQueryable<products> query = db.Products.AsNoTracking().Include(p => p.Brands);
            //pretraga 
            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => EF.Functions.ILike(p.Name, $"{name}%"));

            if (!string.IsNullOrEmpty(brandName))
                query = query.Where(p => EF.Functions.ILike(p.Brands.Name, $"{brandName}%"));

            if (size.HasValue)
                query = query.Where(p => p.Size == size.Value);

            return query;
        }
        public IQueryable<Brands> GetBrands([Service] ProductsDbContext db)
        {
            return db.Brands.AsNoTracking();
        }
    }
}