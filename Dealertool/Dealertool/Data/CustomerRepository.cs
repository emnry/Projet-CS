using Dealertool.Data.InterfaceRepository;
using Dealertool.Model;

namespace Dealertool.Data;

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
    
    // Get Element By email
    
    
    public Customer? GetCustomerByEmail(string email)
    {
        return _dealerDbContext.Customers
            .FirstOrDefault(c => c.Email == email);
    }
    
    // Get Element by ID
    public Customer? GetCustomerById(Guid customerId)
    {
        return _dealerDbContext.Customers.Find(customerId);
    }
    
    // Add new Customer

    public Customer AddCustomer(string firstname, string lastname, DateTime birthdate, string email, string phone, bool save)
    {
        // Verifier si un client à le meme email 
        var existingCustomer = _dealerDbContext.Customers
            .FirstOrDefault(c => c.Email == email);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException("An other customer already owns this email"); // Erreur email --> client deja existant
        }

        // Créer le nouveau client
        Customer customer = new Customer
        {
            Firstname = firstname,
            Lastname = lastname,
            Birthdate = DateTime.SpecifyKind(birthdate, DateTimeKind.Utc), // On force UTC sinon erreur avec PostgreSQL
            Email = email,
            PhoneNumber = phone
        };

        // Ajouter le client à la base
        _dealerDbContext.Customers.Add(customer);
        
        // Ne pas enregistrer par defaut 
        if(save){_dealerDbContext.SaveChanges();}
        

        return customer;
    }
}
