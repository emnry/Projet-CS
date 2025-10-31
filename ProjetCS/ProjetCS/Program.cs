using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjetCS.Data.InterfaceRepository;
using ProjetCS.Model;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));


var configuration = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

String pathCustomer = configuration.GetRequiredSection("CSVFiles")["CSV_Customer"];
String pathVehicle = configuration.GetRequiredSection("CSVFiles")["CSV_Vehicle"];

List<Customer> Customers = new List<Customer>();
List<Vehicle> Vehicles = new List<Vehicle>();



// Import des clients
var customers = new List<Customer>();
using (var reader = new StreamReader(pathCustomer))
{
    string? header = reader.ReadLine(); 
    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine();
        var fields = line.Split('%');
        customers.Add(new Customer
        {
            Id = Guid.NewGuid(),
            Lastname = fields[0],
            Firstname = fields[1],
            Birthdate = DateTimeUtils.ConvertToDateTime(fields[2], "dd/MM/yyyy"),
            PhoneNumber = fields[3],
            Email = fields[4]
        });
    }
}

using (var context = new DealerDbContext())
{
    context.Customers.AddRange(customers);
    context.SaveChanges();
}

// Import des véhicules
var vehicles = new List<Vehicle>();
using (var reader = new StreamReader(pathVehicle))
{
    string? header = reader.ReadLine(); 
    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine();
        var fields = line.Split('/');
        if (fields.Length < 7) continue; 
        vehicles.Add(new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = fields[0],
            Model = fields[1],
            Year = int.Parse(fields[2]),
            PriceExcludingTax = float.Parse(fields[3], CultureInfo.InvariantCulture),
            Color = fields[4],
            Sold = bool.Parse(fields[5]),
            PurchaseDate = DateTimeUtils.ConvertToDateTime(fields[6], "yyyy-MM-dd"),

        });
    }
}

using (var context = new DealerDbContext())
{
    context.Vehicles.AddRange(vehicles);
    context.SaveChanges();
}


























bool running = true;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddDbContext<DealerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // On enregistre notre service applicatif
        services.AddTransient<DbConnection>();
        
        services.AddTransient<IVehicleRepository, VehicleRepository>();
    })
    .Build();

using var scope = host.Services.CreateScope();
IVehicleRepository vehicleRepository = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();
    

while (running)
{
    Console.WriteLine("Enter a command :\n1) Voir liste Voiture\n2) Voir Historique d'achat (croissant)\n3) Ajouter Client\n4) Ajouter Voiture\n5) Faire un achat de Voiture\n6) Fin\n\n>");
    
    string input = Console.ReadLine();
    int command;
    List<Vehicle> vehicles_list;
    
    if (int.TryParse(input, out command))
    {
        switch (command)
        {
            case 1:
                
                Console.WriteLine("--- --- --- --- --- --- --- --- Vehicles list --- --- --- --- --- --- --- ---");
                Console.WriteLine(" Brand / Model / Year Released / Price (VAT Excluded) / Price (VAT Included) / Color / Sold / Buyer Name / Buyer Id");
                vehicles_list = vehicleRepository.GetAllVehicle();

                string id;
                string name;
                foreach (var vehicle in vehicles_list)
                {

                    if (vehicle.Customer == null)
                    {
                        id = "X";
                    }
                    else
                    {
                        id  = vehicle.Customer.Id.ToString();
                    }
                    
                    Console.WriteLine(vehicle.Brand + "/" + vehicle.Model + "/" + vehicle.Year + "/" + vehicle.PriceExcludingTax * 1.2 + "/"+ vehicle.PriceExcludingTax + "/" + vehicle.Color + "/" + vehicle.Sold + "/" + id);
                }
                
                break;
            case 2:
                
                Console.WriteLine("--- --- --- --- --- --- --- --- Purchase History --- --- --- --- --- --- --- ---");
                Console.WriteLine(" Brand / Model / Price (VAT Excluded) / Price (VAT Included) / Color / Sold / Buyer Name / Buyer Id");
                
                vehicles_list = vehicleRepository.GetSales();

                foreach (var vehicle in vehicles_list)
                {
                    Console.WriteLine(vehicle.Brand + "/" + vehicle.Model + "/" + vehicle.PriceExcludingTax * 1.2 + "/"+ vehicle.PriceExcludingTax + "/" + vehicle.Customer.Id.ToString(), vehicle.Customer.Firstname, vehicle.Customer.Lastname, vehicle.Customer.Email);

                }

                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                running = false;
                Console.WriteLine("--- --- --- --- --- --- --- --- Bye --- --- --- --- --- --- --- ---");
                break;
                
        }
    }
    else
    {
        Console.WriteLine("La commande n'est pas valide");
    }
}