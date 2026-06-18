using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Chờ xử lý";

        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
