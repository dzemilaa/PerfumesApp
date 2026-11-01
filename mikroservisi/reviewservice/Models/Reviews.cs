using System.ComponentModel.DataAnnotations;

namespace reviewservice.Models
{
    public class Reviews
    {
        [Key]
        public Guid ReviewId { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
