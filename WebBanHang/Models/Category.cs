using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }

        // Thể hiện một danh mục có thể có nhiều sản phẩm
        public List<Product>? Products { get; set; }
    }
}
