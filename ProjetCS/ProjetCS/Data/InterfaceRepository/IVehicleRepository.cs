using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public interface IVehicleRepository
{
    List<Vehicle> GetAllVehicle();
    public bool CreateVehicle(Vehicle vehicle);
    public bool AddSale(Guid vehicleId, Guid customerId);
    public List<Vehicle> GetSales();
    
}