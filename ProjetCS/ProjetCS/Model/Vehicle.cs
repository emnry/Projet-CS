using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetCS.Model;

public class Vehicle
{
    [Key]
    public Guid Id {get;set;}

    [Required]
    public string Brand { get; set; }

    [Required]
    public string Model { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    public float PriceExcludingTax { get; set; }

    [Required]
    public string Color { get; set; }

    [Required]
    public bool Sold { get; set; } = false;
    public DateTime? PurchaseDate { get; set; }

    [ForeignKey(nameof(Customer))]
    public Guid? IdCustomer { get; set; }

    public Customer? Customer { get; set; }
    
}