using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Repositories;

namespace WebApp.Controllers
{
    public class CategoryController : Controller
    {
            private readonly ICategoryRepository _categoryRepository;
            public CategoryController(ICategoryRepository categoryRepository)
            {
                _categoryRepository = categoryRepository;
            }
            public IActionResult Add()
            {
                return View();
            }
            [HttpPost]
            public IActionResult Add(Category category)
            {
                if (ModelState.IsValid)
                {
                    _categoryRepository.Add(category);
                    return RedirectToAction("Index"); // Chuyển hướng tới trang
                }
                return View(category);
            }
            // Các actions khác như Display, Update, Delete
            // Display a list of products
            public IActionResult Index()
            {
                var categories = _categoryRepository.GetAll();
                return View(categories);
            }

            // Display a single product
            public IActionResult Display(int id)
            {
                var product = _categoryRepository.GetById(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            // Show the product update form
            public IActionResult Update(int id)
            {
                var category = _categoryRepository.GetById(id);
                if (category == null)
                {
                    return NotFound();
                }
                return View(category);
            }
            // Process the product update
            [HttpPost]
            public IActionResult Update(Category category)
            {
                if (ModelState.IsValid)
                {
                _categoryRepository.Update(category);
                    return RedirectToAction("Index");
                }
                return View(category);
            }
            // Show the product delete confirmation
            public IActionResult Delete(int id)
            {
                var product = _categoryRepository.GetById(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            // Process the product deletion
            [HttpPost, ActionName("DeleteConfirmed")]
            public IActionResult DeleteConfirmed(int id)
            {
            _categoryRepository.Delete(id);
                return RedirectToAction("Index");
            }
        }
    }
