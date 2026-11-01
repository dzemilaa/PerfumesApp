using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public interface IProductServiceClient
{
    Task<Dictionary<Guid, ProductDto>> GetProductsByIdsAsync(IEnumerable<Guid> ids);
}

public class ProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("image_url")]
    public string? Image { get; set; }

    public ProductDto() { }

    public ProductDto(Guid id, string? name, decimal price, string? image)
    {
        Id = id;
        Name = name;
        Price = price;
        Image = image;
    }
}