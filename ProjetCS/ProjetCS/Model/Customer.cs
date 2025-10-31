using System.ComponentModel.DataAnnotations;

namespace ProjetCS.Model;

public class Customer
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Firstname { get; set; }

    [Required]
    public string Lastname { get; set; }

    [Required]
    public DateTime Birthdate { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [Required]
    public string Email { get; set; }

    public List<Vehicle> Vehicles { get; set; } = new();
    
    
}