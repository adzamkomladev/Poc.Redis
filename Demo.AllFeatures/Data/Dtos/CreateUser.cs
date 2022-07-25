using System.ComponentModel.DataAnnotations;

namespace Demo.AllFeatures.Data.Dtos;

public class CreateUser
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }
}