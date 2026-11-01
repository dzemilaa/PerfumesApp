namespace productservice.Models
{
    public class Brands
    {
       
            public int Id { get; set; }
            public string Name { get; set; } = null!;
           
            public ICollection<products> Products { get; set; } = new List<products>();
        
    }
}
