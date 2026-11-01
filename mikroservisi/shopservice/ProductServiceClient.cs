using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _http;

    public ProductServiceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<Dictionary<Guid, ProductDto>> GetProductsByIdsAsync(IEnumerable<Guid> ids)
    {
        var idStrings = ids.Select(id => id.ToString()).ToList();

        //koristimo where filter da bi dobili proizvode po id-u
        var gqlQuery = new
        {
            query = @"
            query($ids: [UUID!]) {
              products(where: { id: { in: $ids } }) {
                nodes {
                  id
                  name
                  description
                  price
                  discount
                  stock
                  categoryId
                  brandId
                  genderId
                  imageUrl
                }
              }
            }",
            variables = new { ids = idStrings }
        };

        var response = await _http.PostAsJsonAsync("graphql", gqlQuery);
        response.EnsureSuccessStatusCode();

        using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var products = jsonDoc.RootElement
            .GetProperty("data")
            .GetProperty("products")
            .GetProperty("nodes")
            .EnumerateArray();

        var result = new Dictionary<Guid, ProductDto>();
        foreach (var p in products)
        {
            var id = Guid.Parse(p.GetProperty("id").GetString());
            result[id] = new ProductDto
            {
                Id = id,
                Name = p.GetProperty("name").GetString(),
                Price = p.GetProperty("price").GetDecimal(),
                Image = p.GetProperty("imageUrl").GetString()
            };
        }

        return result;
    }
}