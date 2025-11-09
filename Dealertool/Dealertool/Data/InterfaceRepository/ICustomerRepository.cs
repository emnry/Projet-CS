using Dealertool.Model;

namespace Dealertool.Data.InterfaceRepository;

public interface ICustomerRepository
{
    List<Customer> GetAllCustomer();
    public Customer? GetCustomerById(Guid customerId);
    
    public Customer? GetCustomerByEmail(string email);

    public Customer AddCustomer(string firstname, string lastname, DateTime birthdate, string email, string phone, bool save);
}