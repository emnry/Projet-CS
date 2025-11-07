using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjetCS.Data.InterfaceRepository;
using ProjetCS.Model;

// Chemin relatif vers le dossier du Json 
var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));

// Configuration 
var configuration = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Definition des chemins vers les CSVs
String pathCustomer = configuration.GetRequiredSection("CSVFiles")["CSV_Customer"];
String pathVehicle = configuration.GetRequiredSection("CSVFiles")["CSV_Vehicle"];

List<Customer> Customers = new List<Customer>();
List<Vehicle> Vehicles = new List<Vehicle>();


// Outils
bool running = true;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Pas de "Info"
        logging.ClearProviders();
    })
    
    .ConfigureServices(services =>
    {
        services.AddDbContext<DealerDbContext>(options =>
                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
            ;

        // On enregistre notre service applicatif
        services.AddTransient<DbConnection>();

        services.AddTransient<IVehicleRepository, VehicleRepository>();
    })
    .Build();

using var scope = host.Services.CreateScope();
IVehicleRepository vehicleRepository = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();

string space = new string(' ', 10);
string separation = '\n' + new string('─', 221) + '\n';
Console.Clear();
while (running)
{
    // Affichage commandes + {spaces} espaces

    Console.WriteLine(separation + '\n' +
                      "0) Import the data from the CSVs" + space +
                      "1) View the list of cars" + space +
                      "2) View the sales history" + space +
                      "3) Add a new customer" + space +
                      "4) Add a new car" + space +
                      "5) Make a vehicle purchase" + space +
                      "6) Close the tool" + space +
                      '\n' + separation);

    // Write pour ne pas reecrire sur le texte
    Console.Write("Enter a command nuber : ");

    string input = Console.ReadLine();
    int command;
    Console.WriteLine(' '); // Mise à la ligne après commande

    List<Vehicle> vehicles_list;

    if (int.TryParse(input, out command))
    {
        switch (command)
        {
            case 0:
                // Import des clients
                var customers = new List<Customer>();
                using (var reader = new StreamReader(pathCustomer))
                {
                    string? header = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var fields = line.Split('%');
                        customers.Add(
                            new Customer
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
                    Console.WriteLine("Customers successfully added !");
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
                    Console.WriteLine("Vehicles successfully added !");
                }

                break;

            // Commande 1 : Voir liste voitures
            case 1:

                vehicles_list = vehicleRepository.GetAllVehicle();
                var properties = typeof(Vehicle).GetProperties();


                foreach (var vehicle in vehicles_list)
                {
                    Console.WriteLine($"Identifier : {vehicle.Id}\n" +
                                      $"  | Brand : {vehicle.Brand}\n" +
                                      $"  | Model : {vehicle.Model}\n" +
                                      $"  | Year Released : {vehicle.Year}\n" +
                                      $"  | Net price :  {vehicle.PriceExcludingTax}\n" +
                                      $"  | Gross price : {vehicle.PriceExcludingTax * 1.2}\n" +
                                      $"  | Color :  {vehicle.Color}\n" +
                                      $"  | Sold : {(vehicle.Sold ? "Yes" : "No")}"
                    );
                    if (vehicle.Sold)
                    {
                        Console.WriteLine(
                            $"     | Purchase date : {vehicle.PurchaseDate}\n" +
                            $"     | Customer Identifier : {vehicle.IdCustomer}\n"
                        );
                    }
                }

                Console.WriteLine($"{vehicles_list.Count} vehicles found.");


                break;

            // Voir historique d'achat
            case 2:

                vehicles_list = vehicleRepository.GetSales();

                foreach (var vehicle in vehicles_list)
                {
                    Console.WriteLine($"Identifier : {vehicle.Id}\n" +
                                      $"  | Brand : {vehicle.Brand}\n" +
                                      $"  | Model : {vehicle.Model}\n" +
                                      $"  | Year Released : {vehicle.Year}\n" +
                                      $"  | Net price :  {vehicle.PriceExcludingTax}\n" +
                                      $"  | Gross price : {vehicle.PriceExcludingTax * 1.2}\n" +
                                      $"  | Color :  {vehicle.Color}\n" +
                                      $"  | Customer Identifier : {vehicle.Customer.Id.ToString()}" +
                                      $"  | Customer Firstname : {vehicle.Customer.Firstname}" +
                                      $"  | Customer Lastname : {vehicle.Customer.Lastname}" +
                                      $"  | Customer Email : {vehicle.Customer.Email}" 
                    );
                    
                    
                }

                break;
            // Ajouter Client
            case 3:
                break;
            // Ajouter voiture
            case 4:
                break;
            // Faire achat voiture
            case 5:
                break;
            // Fin
            case 6:
                running = false;
                Console.WriteLine("--- --- --- --- --- --- --- --- Au revoir --- --- --- --- --- --- --- ---");
                break;
        }
    }
    else
    {
        Console.WriteLine("La commande n'est pas valide");
    }
}