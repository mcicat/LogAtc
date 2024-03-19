using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LogAtc.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? ApiPassword { get; set; }
}
