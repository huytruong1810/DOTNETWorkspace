using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOTNETWorkspace.Models;

[Table("People")]
public class Person
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    [StringLength(15)]
    public string? PhoneNumber { get; set; }
}