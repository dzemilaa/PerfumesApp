using System;
using System.Collections.Generic;

namespace shopservice.Models;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = new();
}
