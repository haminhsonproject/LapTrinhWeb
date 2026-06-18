using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        public decimal TotalAmount { get; set; }
    }
}
