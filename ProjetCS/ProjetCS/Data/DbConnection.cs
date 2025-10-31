using Npgsql;
using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public class DbConnection
{
    private readonly DealerDbContext _dealerDbContext;
    public DbConnection(DealerDbContext dealerDbContext)
    {
        _dealerDbContext = dealerDbContext;
    }

    public void SaveFullVehicles(Vehicle myVehicle)
    {
        _dealerDbContext.Add(myVehicle);

        _dealerDbContext.SaveChanges();
    }
}