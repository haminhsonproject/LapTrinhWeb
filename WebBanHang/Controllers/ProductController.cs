using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebBanHang.Models;
using WebBanHang.Repositories;

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Add()
        {
            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            ViewBag.HasCategories = categories.Any();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile? imageUrl)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = (await _categoryRepository.GetAllAsync()).ToList();
            ViewBag.HasCategories = categories.Any();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Product product, IFormFile? imageUrl)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            ModelState.Remove("ImageUrl");
            ModelState.Remove("Category");
            ModelState.Remove("Images");

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                if (imageUrl != null && imageUrl.Length > 0)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = SD.Role_Admin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var savePath = Path.Combine(folderPath, fileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + fileName;
        }
    }
}
