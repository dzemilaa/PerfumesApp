using System;
using System.Collections.Generic;

namespace shopservice.Models;

public class Favorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<FavoriteItem> Items { get; set; } = new();
}
