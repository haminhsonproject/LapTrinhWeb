using Microsoft.EntityFrameworkCore;

namespace WebApp.Models
{
    public class Context : DbContext
    {
        public
        Context(DbContextOptions<Context> options) :
        base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
    }
}
