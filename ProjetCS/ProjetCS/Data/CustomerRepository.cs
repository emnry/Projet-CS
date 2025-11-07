using ProjetCS.Model;

namespace ProjetCS.Data.InterfaceRepository;

public class CustomerRepository : ICustomerRepository
{
    
    
    private readonly DealerDbContext _dealerDbContext;
    
    // Dbcontext
    
    public CustomerRepository(DealerDbContext dealerDbContext)
    {
        _dealerDbContext = dealerDbContext;
    }
    // Get All
    public List<Customer> GetAllCustomer()
    {
        return  _dealerDbContext.Customers.ToList(); 
    }
    
    // Get Element by ID
    public Customer? GetCustomerById(Guid customerId)
    {
        return _dealerDbContext.Customers.Find(customerId);
    }
    
    // Add new Customer

    public bool CreateCustomer(Customer customer)
    {
        _dealerDbContext.Customers.Add(customer);
        _dealerDbContext.SaveChanges();
        return true;
    }
}