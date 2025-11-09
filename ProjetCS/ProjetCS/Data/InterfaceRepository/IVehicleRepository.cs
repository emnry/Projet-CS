using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public interface IVehicleRepository
{
    List<Vehicle> GetAllVehicle();
    public Vehicle AddVehicle(Vehicle vehicle, bool save);
    public bool AddSale(Guid vehicleId, Guid customerId);
    public List<Vehicle> GetSales();

    public Vehicle? GetVehicleById(Guid vehicleId);
    
    
    

}