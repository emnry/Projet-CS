using Microsoft.EntityFrameworkCore;
using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public class DealerDbContext
{
    
    // --- Tables principales ---
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    
    // --- Constructeur ---
    public DealerDbContext(DbContextOptions<DealerDbContext> options)
        : base(options)
    {
    }
    public DealerDbContext() { }
}