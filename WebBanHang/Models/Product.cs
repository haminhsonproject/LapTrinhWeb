using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 100000000000000.00)]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }

        // Các thuộc tính thể hiện mối quan hệ (Navigation properties)
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
