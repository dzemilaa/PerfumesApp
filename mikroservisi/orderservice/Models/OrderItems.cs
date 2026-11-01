namespace orderservice.Models
{
    public class OrderItems
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public string? Image { get; set; }
        public Orders? Orders { get; set; }
    }
}
