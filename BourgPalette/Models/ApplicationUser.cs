using Microsoft.AspNetCore.Identity;

namespace BourgPalette.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
    }
}