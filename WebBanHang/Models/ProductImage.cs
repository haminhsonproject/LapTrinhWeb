namespace WebBanHang.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }

        // Thể hiện hình ảnh này thuộc về sản phẩm nào
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
