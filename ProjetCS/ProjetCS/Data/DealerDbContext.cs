using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public class DealerDbContext : DbContext
{
    
    // --- Tables principales ---
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    
    // --- Constructeur ---
    public DealerDbContext(DbContextOptions<DealerDbContext> options)
        : base(options)
    {}
    
    // Constructeur vide pour EF CLI
    public DealerDbContext() { }
    
    // --- Configuration des relations ---
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relation Customer -> Vehicle (1..n)
        modelBuilder.Entity<Vehicle>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(p => p.IdCustomer);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));
        
        var configuration = new ConfigurationBuilder()
            
            .SetBasePath(projectRoot)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

        }
    }
}