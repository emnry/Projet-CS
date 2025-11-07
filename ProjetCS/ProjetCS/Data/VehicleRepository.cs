using Microsoft.EntityFrameworkCore;
using ProjetCS.Model;
namespace ProjetCS.Data.InterfaceRepository;

public class VehicleRepository : IVehicleRepository
{
    private readonly DealerDbContext _dealerDbContext;

    public VehicleRepository(DealerDbContext dealerDbContext)
    {
        _dealerDbContext = dealerDbContext;
    }

    public List<Vehicle> GetAllVehicle()
    {
        return _dealerDbContext.Vehicles.ToList();
    }

    public List<Vehicle> GetSales()
    {
        return _dealerDbContext.Vehicles
            .Where(vehicle => vehicle.Sold == true)
            .Include(vehicle => vehicle.Customer)
            .OrderBy(vehicle => vehicle.PurchaseDate)
            .ToList();
    }

    public bool AddSale(Guid vehicleId, Guid customerId)
    {
        var vehicle = _dealerDbContext.Vehicles.Where(vehicle => vehicle.Id == vehicleId).FirstOrDefault();

        if (vehicle != null)
        {
            vehicle.Sold = true;
            vehicle.PurchaseDate = DateTime.Now;
            vehicle.IdCustomer = customerId;

            _dealerDbContext.SaveChanges();
        }
        else
        {
            Console.WriteLine($"Vehicle with ID {vehicleId} not found.");

            return false;
        }

        return true;
    }

    public bool CreateVehicle(Vehicle vehicle)
    {
        _dealerDbContext.Vehicles.Add(vehicle);
        _dealerDbContext.SaveChanges();
        return true;
    }
}