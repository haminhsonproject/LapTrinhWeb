using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebBanHang.Models;
using WebBanHang.Repositories; // Thêm thư viện Repository

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        // Tiêm IProductRepository vào HomeController
        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ sản phẩm và gửi sang View trang chủ
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}