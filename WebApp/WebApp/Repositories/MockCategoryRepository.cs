using WebApp.Models;

namespace WebApp.Repositories;

    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categorylist;
        public MockCategoryRepository() {
        _categorylist = new List<Category>
            {
                new Category { Id = 1, Name = "Laptop"},
                new Category { Id = 2, Name = "Mouse"}
            };
        }
    public IEnumerable<Category> GetAll()
    {
        return _categorylist;
    }
    public Category GetById(int id)
    {
        return _categorylist.FirstOrDefault(p => p.Id == id)
            ?? throw new Exception($"Không tìm thấy sản phẩm nào với ID = {id}");
    }
    public void Add(Category category)
    {
        category.Id = _categorylist.Max(p => p.Id) + 1;
        _categorylist.Add(category);
    }
    public void Update(Category category)
    {
        var index = _categorylist.FindIndex(p => p.Id == category.Id);
        if (index != -1)
        {
            _categorylist[index] = category;
        }
    }
    public void Delete(int id)
    {
        var product = _categorylist.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _categorylist.Remove(product);
        }
    }
}

