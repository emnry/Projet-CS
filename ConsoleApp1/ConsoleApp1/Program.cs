using Spectre.Console;

var choice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select a vehicle action:")
        .AddChoices(new[] { "View Details", "Sell Vehicle", "Delete Vehicle" })
);

AnsiConsole.MarkupLine($"You selected: [green]{choice}[/]");