#region Dependances

using System.Globalization;
using Dealertool.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dealertool.Data.InterfaceRepository;
using Dealertool.Model;
using Spectre.Console;

#endregion

#region Configuration

// Pour avoir "€ et autres symboles si besoin"
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Chemin relatif Json 
var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\"));

// Configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Definition des chemins vers les CSVs
String pathCustomer = configuration.GetRequiredSection("CSVFiles")["CSV_Customer"];
String pathVehicle = configuration.GetRequiredSection("CSVFiles")["CSV_Vehicle"];

// Toggle de boucle principale 
bool running = true;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // Pas de Info SQL
        logging.ClearProviders();
    })
    .ConfigureServices(services =>
    {
        services.AddDbContext<DealerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<DbConnection>();

        services.AddTransient<IVehicleRepository, VehicleRepository>();
        services.AddTransient<ICustomerRepository, CustomerRepository>();
    })
    .Build();

using var scope = host.Services.CreateScope();
IVehicleRepository vehicleRepository = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();
ICustomerRepository customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();

#endregion

#region Titre et Valeurs OuterScope

// Vide le terminal
Console.Clear();

// Mise à la ligne
AnsiConsole.WriteLine();

// Titre ASCII
var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "basic.flf");
var font = FigletFont.Load(fontPath);

AnsiConsole.Write(
    new FigletText(font, "[ DEALER TOOL ]")
        .Centered()
);



string GetSpectreColor(string color)
{
    // Couleur des valeurs dans la colonne "Color" de la table vehicle
    Dictionary<string, string> colorTraduction = new()
    {
        { "orange", "orange1" },
        { "pink", "pink1" },
        { "turquoise", "turquoise4" },
        { "beige", "tan" },
        { "ivory", "tan" },
        { "gold", "gold1" },
    };

    HashSet<string> spectreColors = new()
    {
        "black", "white", "red", "green", "blue", "yellow", "grey", "pink", "purple", "cyan", "magenta", "tan", "gold1", "silver", "orange1", "turquoise4"
    };

    string inputColor = color.ToLower();
    return colorTraduction.ContainsKey(inputColor)
        ? colorTraduction[inputColor]
        : (spectreColors.Contains(inputColor) ? inputColor : "white");
}

var context = scope.ServiceProvider.GetRequiredService<DealerDbContext>();

#endregion

#region Fonctions

string InputValidString(string message, string valueName)
{
    string valueString;
    do
    {
        AnsiConsole.MarkupLine($"[bold]{message}[/]");
        valueString = Console.ReadLine() ?? "";
        
        if (!string.IsNullOrWhiteSpace(valueString))
            break;

        AnsiConsole.MarkupLine($"[red]{valueName} cannot be empty.[/]");
        
    } while (string.IsNullOrWhiteSpace(valueString));
    
    return  valueString;
}


Customer AddNewCustomer()
{
    string firstname = InputValidString("Enter the client Firstname: ", "Firstname");
    string lastname = InputValidString("Enter the client Lastname: ", "Lastname");

    DateTime birthdate;
    while (true)
    {
        AnsiConsole.MarkupLine("[bold]Enter the client Birthdate (dd/MM/yyyy): [/]");
        string birthInput = Console.ReadLine() ?? "";

        if (DateTime.TryParseExact(birthInput, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdate))
        {
            birthdate = DateTime.SpecifyKind(birthdate, DateTimeKind.Utc); // Convertion UTC pour postgres
            break; // format valide 
        }

        AnsiConsole.MarkupLine("[red]Invalid date format. Please try again.[/]");
    }

    string email = InputValidString("Enter the client Email: ", "Email");
    string phone = InputValidString("Enter the client Phone number: ", "Phone number");

    AnsiConsole.MarkupLine("[green]Customer successfully created![/]");
    
    return customerRepository.AddCustomer(firstname, lastname, birthdate, email, phone, true);
}

Vehicle AddNewVehicle()
{
    string brand = InputValidString("Enter the vehicle Brand: ", "Brand");
    string model = InputValidString("Enter the vehicle Model: ", "Model");
    string color = InputValidString("Enter the vehicle Color (FR): ", "Color");
    
    int year;
    do
    {
        AnsiConsole.MarkupLine("[bold]Enter vehicle Release Year: [/]");
        string? input = Console.ReadLine();
    
        // Vérifie que pas vide et entier 
        if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out year))
        {
            break;
        }

        AnsiConsole.MarkupLine("[red]Invalid input, please enter a valid year.[/]");
        
    } while (true);
    
    float grossPrice;
    do
    {
        AnsiConsole.MarkupLine("[bold]Enter vehicle Gross Price: (XXXX,XX)[/]");

        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input) && float.TryParse(input, out grossPrice))
            break;

        AnsiConsole.MarkupLine("[red]Invalid input, please enter a valid price (number).[/]");
    } while (true);

    Vehicle vehicleElement = new Vehicle
    {
        Brand = brand,
        Model = model,
        Year = year,
        PriceExcludingTax = grossPrice,
        Color = char.ToUpper(color[0]) + color.Substring(1).ToLower(), // --> 1ere en MAJ sinon Min 
        Sold = false
    };
    
    AnsiConsole.MarkupLine("[green]Vehicle successfully created![/]");

    return vehicleRepository.AddVehicle(vehicleElement, true);
    
    
    
}
// Pour ajouter les columnes à partir d'une liste de str --> permet de moins se repeter 
void AddColumnfromlist(Table table, List<string> list)
{
    foreach (string item in list)
    {
        table.AddColumn($"[bold]{item}[/]").Centered();
    }
}

#endregion

#region Test

// Reset des tables
//context.Database.EnsureDeleted();
//context.Database.EnsureCreated();

#endregion


#region Boucle principale

while (running)
{
    // Variables InnerScoper
    List<Vehicle> vehicleList;
    Vehicle? vehicleElement = null;

    List<Customer> customerList;
    Customer? customerElement = null;

    var table = new Table();
    table.Border(TableBorder.Rounded);
    table.Expand(); // Prend toute la largeur de la fenetre

    List<string> columns;

    #region Menu principal

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select an option :")
            .MoreChoicesText("[grey](Use arrow keys to navigate)[/]")
            .PageSize(20)
            .HighlightStyle(new Style(Color.White, decoration: Decoration.Underline))
            .AddChoices(new[]
            {
                "[grey]Import the data from the CSVs[/]",

                "[yellow]View the list of cars[/]",
                "[yellow]View the list of customers[/]",

                "[blue]View the sales history[/]",
                "[blue]Make a vehicle purchase[/]",

                "[magenta]Add a new customer[/]",
                "[magenta]Add a new vehicle[/]",

                "[green]Find a customer by his identifier[/]",
                "[green]Find a vehicle by his identifier[/]",
                "[green]Find customer's vehicles[/]",
                "[green]Find a customer by his email[/]",


                "[red]Close the tool[/]"
            }));

    #endregion


    #region Gestion des choix

    switch (choice)
    {
        #region Import the data from the CSVs

        case var s when s.Contains("Import the data from the CSVs"):

            // Import des clients
            if (string.IsNullOrEmpty(pathCustomer))
            {
                break;
            }

            var lines = File.ReadAllLines(pathCustomer);

            for (int i = 1; i < lines.Length; i++)
            {
                String line = lines[i];
                var fields = line.Split('%');
                
               
                // Parser
                DateTime date = DateTime.ParseExact(fields[2], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                // Vers UTC pour PostgresSQL
                DateTime utcDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);

                customerRepository.AddCustomer(fields[0], fields[1], utcDate, fields[4], fields[3], false);
            }

            context.SaveChanges();
            AnsiConsole.MarkupLine("[green]Customers successfully added ![/]");

            // Import des véhicules
            if (string.IsNullOrEmpty(pathVehicle))
            {
                break;
            }

            lines = File.ReadAllLines(pathVehicle);


            for (int i = 1; i < lines.Length; i++)
            {
                String line = lines[i];
                var fields = line.Split('/');
                

                vehicleElement = new Vehicle
                {
                    Brand = fields[0],
                    Model = fields[1],
                    Year = int.Parse(fields[2]),
                    PriceExcludingTax = float.Parse(fields[3], CultureInfo.InvariantCulture),
                    Color = char.ToUpper(fields[4][0]) + fields[4].Substring(1).ToLower(),
                    Sold = bool.Parse(fields[5]),
                    PurchaseDate = DateTime.SpecifyKind(
                        DateTimeUtils.ConvertToDateTime(fields[6], "yyyy-MM-dd"),
                        DateTimeKind.Utc
                    )
                };

                if (vehicleElement.Sold)
                {
                    Customer? customer = customerRepository.GetCustomerByEmail(fields[7]);
                    if (customer != null)
                    {
                        vehicleElement.IdCustomer = customer.Id;
                        vehicleElement.Customer = customer;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]No matching customer for email {fields[7]}[/]");
                    }
                }


                vehicleRepository.AddVehicle(vehicleElement, false);
            }

            context.SaveChanges();
            AnsiConsole.MarkupLine("[green]Vehicles successfully added ![/]");


            break;

        #endregion

        #region View the list of cars

        case var s when s.Contains("View the list of cars"):

            vehicleList = vehicleRepository.GetAllVehicle();

            columns = new List<string>
            {
                "Identifier", "Brand", "Model", "Year Released", "Net price", "Gross price", "Color", "Sold",
                "Purchase date", "Customer Identifier"
            };

            AddColumnfromlist(table, columns);

            foreach (Vehicle vehicle in vehicleList)
            {
                // Appel de la fonction de vérification de la couleur
                string couleurSpectre = GetSpectreColor(vehicle.Color);




                table.AddRow(
                    vehicle.Id.ToString(),
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year.ToString(),
                    (vehicle.PriceExcludingTax * 1.2).ToString("0.00") + " €",
                    vehicle.PriceExcludingTax.ToString("0.00") + " €",
                    $"[{couleurSpectre}]{vehicle.Color}[/]",
                    vehicle.Sold ? "[green]Yes[/]" : "[Red]No[/]",

                    // Si vehicle_sold alors ajout de la date d'achat et idcustomer + gestion des erreurs si null mais sold true
                    vehicle.Sold ? vehicle.PurchaseDate.ToString() ?? "" : "",
                    vehicle.Sold ? vehicle.IdCustomer.ToString() ?? "" : ""
                );
            }

            // Affichage de la table
            AnsiConsole.Write(table);

            Console.WriteLine($"{vehicleList.Count} vehicles found.");

            break;

        #endregion

        #region View the list of customers

        case var s when s.Contains("View the list of customers"):


            customerList = customerRepository.GetAllCustomer();

            columns = new List<string>()
            {
                "Identifier", "Firstname", "Lastname", "Birth date", "Email", "Phone number"
            };

            AddColumnfromlist(table, columns);

            foreach (Customer customer in customerList)
            {
                table.AddRow(
                    customer.Id.ToString(),
                    customer.Firstname,
                    customer.Lastname,
                    customer.Birthdate.ToString("dd/MM/yyyy"),
                    customer.Email,
                    customer.PhoneNumber
                );
            }

            // Affichage de la table
            AnsiConsole.Write(table);

            Console.WriteLine($"{customerList.Count} customers found.");

            break;

        #endregion

        #region View the sales history

        case var s when s.Contains("View the sales history"):
            vehicleList = vehicleRepository.GetSales();

            columns = new List<string>()
            {
                "Identifier", "Brand", "Model", "Year Released", "Net price", "Gross price", "Color", "Purchase Date",
                "Customer Identifier", "Customer Firstname", "Customer Lastname", "Customer Email",
                "Customer Phone number"
            };

            AddColumnfromlist(table, columns);

            foreach (Vehicle vehicle in vehicleList)
            {
                // Appel de la fonction de vérification de la couleur
                string couleurSpectre = GetSpectreColor(vehicle.Color);


                table.AddRow(
                    vehicle.Id.ToString(),
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year.ToString(),
                    vehicle.PriceExcludingTax.ToString("0.00") + " €",
                    (vehicle.PriceExcludingTax * 1.2).ToString("0.00") + " €",
                    $"[{couleurSpectre}]{vehicle.Color}[/]",
                    vehicle.PurchaseDate?.ToString("yyyy-MM-dd") ?? " ",
                    vehicle.IdCustomer.ToString() ?? " ",
                    vehicle.Customer?.Firstname ?? " ",
                    vehicle.Customer?.Lastname ?? " ",
                    vehicle.Customer?.Email ?? " ",
                    vehicle.Customer?.PhoneNumber ?? " "
                );
            }

            // Affichage de la table
            AnsiConsole.Write(table);

            Console.WriteLine($"{vehicleList.Count} vehicles found.");


            break;

        #endregion

        case var s when s.Contains("Make a vehicle purchase"):
            var confirmation = AnsiConsole.Prompt(
                new TextPrompt<bool>("Does the client have an account ?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n"));
            switch (confirmation)
            {
                case true:
                    do
                    {
                        AnsiConsole.MarkupLine("[bold]Enter client email (or type 'exit' to cancel) : [/]");
                        string input = Console.ReadLine() ?? "";
                        if (input == "exit")
                        {
                            AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                            break;
                        }

                        customerElement = customerRepository.GetCustomerByEmail(input);
                    } while (customerElement == null);

                    break;

                case false:
                    customerElement = AddNewCustomer();
                    break;
            }

            if (customerElement == null)
            {
                break;
            }

            do
            {
                AnsiConsole.MarkupLine("[bold]Enter vehicle Identifier (or type 'exit' to cancel) : [/]");
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                    break;
                }

                if (Guid.TryParse(input, out Guid vehicleId))
                {
                    vehicleElement = vehicleRepository.GetVehicleById(vehicleId);

                    if (vehicleElement == null)
                        AnsiConsole.MarkupLine("[red]Vehicle not found, try again.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid GUID format, try again.[/]");
                }
            } while (vehicleElement == null);

            if (vehicleElement == null)
            {
                break;
            }

            vehicleRepository.AddSale(vehicleElement.Id, customerElement.Id);
            AnsiConsole.MarkupLine("[green]Purchase successfully saved ![/]");

            columns = new List<string>
            {
                "Field", "Detail"
            };

            AddColumnfromlist(table, columns);

            table.AddRow("Client Name", $"{customerElement.Firstname} {customerElement.Lastname}");
            table.AddRow("✉ Client Email", customerElement.Email);
            table.AddRow("☎ Client Phone number", customerElement.PhoneNumber);
            table.AddRow("");
            table.AddRow("Vehicle", $"{vehicleElement.Brand} {vehicleElement.Model} ({vehicleElement.Year})");
            table.AddRow("Identifier", vehicleElement.Id.ToString());
            table.AddRow("Net Price", $"{vehicleElement.PriceExcludingTax:0.00} €");
            table.AddRow("Gross Price", $"{vehicleElement.PriceExcludingTax * 1.2:0.00} €");
            table.AddRow("Price (Gross, TTC)", $"{vehicleElement.PriceExcludingTax * 1.2:0.00} €");
            table.AddRow("");
            table.AddRow("Purchase Date", $"{vehicleElement.PurchaseDate:yyyy-MM-dd HH:mm:ss}");
            
            AnsiConsole.Write(table);
            
            break;
        case var s when s.Contains("Add a new customer"):
            customerElement = AddNewCustomer();
            break;

        case var s when s.Contains("Add a new vehicle"):
            vehicleElement = AddNewVehicle();
            break;
        
        case var s when s.Contains("Find a customer by his identifier"):
            AnsiConsole.MarkupLine("[bold]Enter the customer identifier (or type 'exit' to cancel): [/]");
            string inputCustomerId = Console.ReadLine() ?? "";
            if (inputCustomerId == "exit")
            {
                AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                break;
            }

            if (!Guid.TryParse(inputCustomerId, out Guid customerId))
            {
                AnsiConsole.MarkupLine("[red]Invalid GUID format. Try again.[/]");
                break;
            }

            customerElement = customerRepository.GetCustomerById(customerId);

            if (customerElement == null)
            {
                AnsiConsole.MarkupLine("[red]Customer not found.[/]");
                break;
            }

            columns = new List<string>()
            {
                "Identifier", "Firstname", "Lastname", "Birth date", "Email", "Phone number"
            };

            AddColumnfromlist(table, columns);

            table.AddRow(
                customerElement.Id.ToString(),
                customerElement.Firstname,
                customerElement.Lastname,
                customerElement.Birthdate.ToString("dd/MM/yyyy"),
                customerElement.Email,
                customerElement.PhoneNumber
            );

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[green]Customer found and displayed![/]");
            break;


        case var s when s.Contains("Find a vehicle by his identifier"):
            AnsiConsole.MarkupLine("[bold]Enter the vehicle identifier (or type 'exit' to cancel): [/]");
            string inputVehId = Console.ReadLine() ?? "";
            if (inputVehId == "exit")
            {
                AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                break;
            }

            if (!Guid.TryParse(inputVehId, out Guid vehId))
            {
                AnsiConsole.MarkupLine("[red]Invalid GUID format. Try again.[/]");
                break;
            }

            vehicleElement = vehicleRepository.GetVehicleById(vehId);

            if (vehicleElement == null)
            {
                AnsiConsole.MarkupLine("[red]Vehicle not found.[/]");
                break;
            }

            columns = new List<string>
            {
                "Identifier", "Brand", "Model", "Year Released", "Net price", "Gross price", "Color", "Sold", "Purchase date", "Customer Identifier"
            };

            AddColumnfromlist(table, columns);

            string colorDisplay = GetSpectreColor(vehicleElement.Color);

            table.AddRow(
                vehicleElement.Id.ToString(),
                vehicleElement.Brand,
                vehicleElement.Model,
                vehicleElement.Year.ToString(),
                (vehicleElement.PriceExcludingTax * 1.2).ToString("0.00") + " €",
                vehicleElement.PriceExcludingTax.ToString("0.00") + " €",
                $"[{colorDisplay}]{vehicleElement.Color}[/]",
                vehicleElement.Sold ? "[green]Yes[/]" : "[Red]No[/]",
                vehicleElement.Sold ? vehicleElement.PurchaseDate.ToString() ?? "" : "",
                vehicleElement.Sold ? vehicleElement.IdCustomer.ToString() ?? "" : ""
            );
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[green]Vehicle found and displayed![/]");
            break;


        case var s when s.Contains("Find customer's vehicles"):
            AnsiConsole.MarkupLine("[bold]Enter the customer identifier (or type 'exit' to cancel): [/]");
            string inputId = Console.ReadLine() ?? "";
            if (inputId == "exit")
            {
                AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                break;
            }

            if (!Guid.TryParse(inputId, out Guid idcustomer))
            {
                AnsiConsole.MarkupLine("[red]Invalid GUID format. Try again.[/]");
                break;
            }

            customerElement = customerRepository.GetCustomerById(idcustomer);

            if (customerElement == null)
            {
                AnsiConsole.MarkupLine("[red]Customer not found.[/]");
                break;
            }

            vehicleList = vehicleRepository.GetVehiclesByCustomerId(idcustomer); // Utilise ta fonction déjà écrite
            if (vehicleList == null || vehicleList.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No vehicles found for this customer.[/]");
                break;
            }

            columns = new List<string>
            {
                "Identifier", "Brand", "Model", "Year Released", "Net price", "Gross price", "Color", "Sold", "Purchase date"
            };
            AddColumnfromlist(table, columns);

            foreach (Vehicle vehicle in vehicleList)
            {
                string couleurSpectre2 = GetSpectreColor(vehicle.Color);
                table.AddRow(
                    vehicle.Id.ToString(),
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year.ToString(),
                    (vehicle.PriceExcludingTax * 1.2).ToString("0.00") + " €",
                    vehicle.PriceExcludingTax.ToString("0.00") + " €",
                    $"[{couleurSpectre2}]{vehicle.Color}[/]",
                    vehicle.Sold ? "[green]Yes[/]" : "[Red]No[/]",
                    vehicle.PurchaseDate?.ToString("yyyy-MM-dd") ?? ""
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[green]Vehicles for this customer found and displayed![/]");
            break;
        
        case var s when s.Contains("Find a customer by his email"):
            string inputEmail = InputValidString("Enter the customer email (or type 'exit' to cancel): ", "Email");
            if (inputEmail.ToLower() == "exit")
            {
                AnsiConsole.MarkupLine("[red]Operation cancelled by user[/]");
                break;
            }

            customerElement = customerRepository.GetCustomerByEmail(inputEmail);

            if (customerElement == null)
            {
                AnsiConsole.MarkupLine("[red]Customer not found.[/]");
                break;
            }

            columns = new List<string>()
            {
                "Identifier", "Firstname", "Lastname", "Birth date", "Email", "Phone number"
            };

            AddColumnfromlist(table, columns);

            table.AddRow(
                customerElement.Id.ToString(),
                customerElement.Firstname,
                customerElement.Lastname,
                customerElement.Birthdate.ToString("dd/MM/yyyy"),
                customerElement.Email,
                customerElement.PhoneNumber
            );

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[green]Customer found and displayed![/]");
            break;


        case var s when s.Contains("Close the tool"):
            running = false;
            Console.WriteLine("Tool closed.");
            break;
    }

    #endregion
}

#endregion