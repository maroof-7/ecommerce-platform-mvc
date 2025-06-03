using System.ComponentModel.DataAnnotations;

namespace DummyProject.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;  // Avoid null issue

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;  // Avoid null issue

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;  // Avoid null issue

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }  // Nullable because it's optional
        public string? Username { get; internal set; }
    }
}
