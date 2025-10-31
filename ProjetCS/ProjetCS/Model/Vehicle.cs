using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetCS.Model;

public class Vehicle
{
    [Key]
    public Guid Id {get;set;} = new Guid();
    
    [Required]
    private string firstname;
    
    [Required]
    private string lastname;
    
    [Required]
    private DateTime birthdate;
    
    [Required]
    private string phone_number;
    
    [Required]
    private string email;
    
    [ForeignKey("customer_vehicle_fk")]
    public Guid IdCustomer {get; set;}
    
    public Customer? Customer {get; set;}
    
    
}