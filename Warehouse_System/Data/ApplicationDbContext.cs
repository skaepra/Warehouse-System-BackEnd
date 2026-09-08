using Microsoft.EntityFrameworkCore;

namespace Online_Store_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. ضبط علاقة One-to-Many بين Product و ProductVariant
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // حذف المتغيرات تلقائياً عند حذف المنتج الأساسي

            // 2. تحويل قائمة الصور العامة DefaultImages لـ JSON
            modelBuilder.Entity<Product>()
                .Property(p => p.DefaultImages)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<string>()
                );

            // 3. تحويل قائمة صور المتغير Images لـ JSON
            modelBuilder.Entity<ProductVariant>()
                .Property(v => v.Images)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null!) ?? new List<string>()
                );
        }
    }
}