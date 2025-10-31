using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public interface ICustomerRepository
{
    List<Customer> GetAllCustomer();
}