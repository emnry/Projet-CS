using Microsoft.Extensions.Configuration;
using ProjetCS.Model;

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));


var configuration = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

String pathCustomer = configuration.GetRequiredSection("CSVFiles")["CSV_Customer"];
String pathVehicle = configuration.GetRequiredSection("CSVFiles")["CSV_Vehicle"];

List<Customer> Vehicles = new List<Customer>();
List<Vehicle> Customers = new List<Vehicle>();

