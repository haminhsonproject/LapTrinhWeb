using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Extensions;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.Product.Id == id);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = product,
                    Quantity = 1
                });
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == id);

            if (item != null)
            {
                item.Quantity++;
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }

                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var model = new CheckoutViewModel
            {
                CustomerName = currentUser?.FullName ?? currentUser?.Email ?? string.Empty,
                PhoneNumber = currentUser?.PhoneNumber ?? string.Empty,
                ShippingAddress = currentUser?.Address ?? string.Empty,
                PaymentMethod = "COD",
                TotalAmount = CalculateTotal(cart)
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng đang trống.";
                return RedirectToAction(nameof(Index));
            }

            model.TotalAmount = CalculateTotal(cart);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var order = new Order
            {
                UserId = currentUser.Id,
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                ShippingAddress = model.ShippingAddress,
                Note = model.Note,
                PaymentMethod = model.PaymentMethod,
                Status = model.PaymentMethod == "COD" ? "Chờ xử lý" : "Đã thanh toán",
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                OrderDetails = cart.Select(item => new OrderDetail
                {
                    ProductId = item.Product.Id,
                    ProductName = item.Product.Name,
                    Price = item.Product.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);
            return RedirectToAction(nameof(OrderCompleted), new { id = order.Id });
        }

        [Authorize]
        public async Task<IActionResult> OrderCompleted(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null || order.UserId != currentUser.Id)
            {
                return NotFound();
            }

            return View(order);
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.Set(CartSessionKey, cart);
        }

        private static decimal CalculateTotal(List<CartItem> cart)
        {
            return cart.Sum(item => item.Product.Price * item.Quantity);
        }
    }
}
