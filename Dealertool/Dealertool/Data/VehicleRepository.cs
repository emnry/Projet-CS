using Dealertool.Data.InterfaceRepository;
using Dealertool.Model;
using Microsoft.EntityFrameworkCore;

namespace Dealertool.Data;

public class VehicleRepository : IVehicleRepository
{
    private readonly DealerDbContext _dealerDbContext;

    // DBContext
    public VehicleRepository(DealerDbContext dealerDbContext)
    {
        _dealerDbContext = dealerDbContext;
    }

    // Get All
    public List<Vehicle> GetAllVehicle()
    {
        return _dealerDbContext.Vehicles.ToList();
    }

    // Get Element by ID
    public Vehicle? GetVehicleById(Guid vehicleId)
    {
        return _dealerDbContext.Vehicles.Find(vehicleId);
    }
    
    // Get All Sold

    public List<Vehicle> GetSales()
    {
        return _dealerDbContext.Vehicles
            .Where(vehicle => vehicle.Sold == true)
            .Include(vehicle => vehicle.Customer)
            .OrderBy(vehicle => vehicle.PurchaseDate)
            .ToList();
    }

    // Add new Sale (link between Vehicle and Customer)
    public bool AddSale(Guid vehicleId, Guid customerId)
    {
        Vehicle vehicle = _dealerDbContext.Vehicles.Where(vehicle => vehicle.Id == vehicleId).FirstOrDefault();

        if (vehicle != null)
        {
            vehicle.Sold = true;
            vehicle.PurchaseDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc); // date actuelle utc
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

    // Add new Vehicle
    public Vehicle AddVehicle(Vehicle vehicle, bool save )
    {
        _dealerDbContext.Vehicles.Add(vehicle);
        
        if(save){
            _dealerDbContext.SaveChanges();
        }

        return vehicle;
    }
    
    // Get a vehicule by customer id
    public List<Vehicle> GetVehiclesByCustomerId(Guid customerId)
    {
        return _dealerDbContext.Vehicles.Where(v => v.IdCustomer == customerId).ToList();
    }

}