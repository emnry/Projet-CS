using System.ComponentModel.DataAnnotations;

namespace ProjetCS.Model;

public class Customer
{
    [Key]
    public Guid Id {get;set;} = new Guid();
    
    [Required]
    private string brand;
    
    [Required]
    private string model;
    
    [Required]
    private int year;
    
    [Required]
    private float price_excluding_tax;
    
    [Required]
    private string color;
    
    [Required]
    private bool sold = false;
    
    private List<Vehicle> vehicles;
    
    
}