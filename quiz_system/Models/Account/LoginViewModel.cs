using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace quiz_system.Models.Account
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Remote(action: "IsUserRegistered", controller: "Account")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
