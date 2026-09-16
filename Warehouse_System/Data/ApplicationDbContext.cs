using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Store_Backend.Table;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 



namespace Online_Store_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<InventoryAudit> InventoryAudits { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Salesperson)
                .WithMany()
                .HasForeignKey(o => o.SalespersonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryAudit>()
                .HasOne(a => a.Storekeeper)
                .WithMany()
                .HasForeignKey(a => a.StorekeeperId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryAudit>()
                .HasOne(a => a.Manager)
                .WithMany()
                .HasForeignKey(a => a.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
               .HasOne(i => i.IssuedBy)
               .WithMany()
               .HasForeignKey(i => i.IssuedById)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}