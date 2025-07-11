using System.ComponentModel.DataAnnotations;
using DummyProject.Models.DomainModel;
using DummyProject.Types;

namespace DummyProject.Models.DomainModel
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        public required string Username { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required Role Role { get; set; } = Role.Buyer;
        public string? ProfilePictureUrl { get; set; } = "dummy Url";

        // One-to-one relationship with Cart
        public Cart? Cart { get; set; }

        // One-to-many relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; } = DateTime.UtcNow;
        public string? FirstName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LastName { get; set; }

        
    }
    
}