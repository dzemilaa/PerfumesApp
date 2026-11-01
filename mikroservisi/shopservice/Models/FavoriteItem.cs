using System;

namespace shopservice.Models;

public class FavoriteItem
{
    public Guid Id { get; set; }
    public Guid FavoriteId { get; set; }
    public Guid ProductId { get; set; }
    public Favorite Favorite { get; set; }
}
