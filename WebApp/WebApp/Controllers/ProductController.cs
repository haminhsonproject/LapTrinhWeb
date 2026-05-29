using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Repositories;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // Hiển thị danh sách sản phẩm
    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        return View(products);
    }

    // Hiển thị form thêm sản phẩm mới
    public async Task<IActionResult> Add()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    // Xử lý thêm sản phẩm mới
    [HttpPost]
    public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
    {
        // FIX: Xóa bỏ kiểm tra validation tự động cho các trường không nhập trực tiếp từ text đầu vào
        ModelState.Remove("ImageUrl");
        ModelState.Remove("Category");

        if (ModelState.IsValid)
        {
            if (imageUrl != null)
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            await _productRepository.AddAsync(product);
            return RedirectToAction(nameof(Index));
        }

        // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(product);
    }


    private async Task<string> SaveImage(IFormFile image)
    {
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

        // FIX: Tự động tạo thư mục images nếu chưa có để tránh lỗi DirectoryNotFoundException
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // FIX: Tạo tên file duy nhất bằng Guid để tránh trùng tên ảnh làm đè file cũ
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
        string savePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(savePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        return "/images/" + uniqueFileName;
    }

    // Hiển thị thông tin chi tiết sản phẩm
    public async Task<IActionResult> Display(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // Hiển thị form cập nhật sản phẩm
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

    // Xử lý cập nhật sản phẩm
    [HttpPost]
    public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
    {
        // FIX: Loại bỏ xác thực ModelState cho các thực thể điều hướng để IsValid không bị ép thành false
        ModelState.Remove("ImageUrl");
        ModelState.Remove("Category");

        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);

            // FIX: Kiểm tra xem sản phẩm gốc trong database có tồn tại hay không trước khi đọc/ghi tiếp
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (imageUrl == null)
            {
                product.ImageUrl = existingProduct.ImageUrl;
            }
            else
            {
                product.ImageUrl = await SaveImage(imageUrl);
            }

            // Cập nhật các thông tin khác của sản phẩm
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.ImageUrl = product.ImageUrl;

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    // Hiển thị form xác nhận xóa sản phẩm
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    // Xử lý xóa sản phẩm công thức POST chuẩn
    [HttpPost, ActionName("DeleteConfirmed")] // FIX: Đổi ActionName thành "Delete" cho khớp logic định tuyến từ View gửi lên
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _productRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}