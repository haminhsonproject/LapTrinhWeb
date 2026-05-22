using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Repositories;

namespace WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // GET: Add
        [HttpGet]
        public IActionResult Add()
        {
            var categories = _categoryRepository.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Add
        [HttpPost]
        public async Task<IActionResult> Add(Product product,
            IFormFile? imageUrl, List<IFormFile>? imageUrls)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                    product.ImageUrl = await SaveImage(imageUrl);

                if (imageUrls != null && imageUrls.Count > 0)
                {
                    product.ImageUrls = new List<string>();
                    foreach (var file in imageUrls)
                        product.ImageUrls.Add(await SaveImage(file));
                }

                _productRepository.Add(product);
                return RedirectToAction("Index");
            }

            var categories = _categoryRepository.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // GET: Index
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        // GET: Display
        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Update ← load form
        [HttpGet]
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();

            var categories = _categoryRepository.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // POST: Update ← xử lý submit
        [HttpPost]
        public async Task<IActionResult> Update(Product product, IFormFile? imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                    product.ImageUrl = await SaveImage(imageUrl);

                _productRepository.Update(product);
                return RedirectToAction("Index");
            }

            var categories = _categoryRepository.GetAll();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // GET: Delete
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);
            return RedirectToAction("Index");
        }

        // Helper: Save image
        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var savePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }
    }
}