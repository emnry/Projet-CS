using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public interface IVehicleRepository
{
    List<Vehicle> GetAllVehicle();
    List<Vehicle> GetVehiclesByCustomerId(Guid customerId);
}