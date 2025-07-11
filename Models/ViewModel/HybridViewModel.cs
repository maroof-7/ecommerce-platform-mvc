using DummyProject.Models.DomainModel;
using DummyProject.Models.JunctionModels;

namespace DummyProject.Models.ViewModel
{
    public class HybridViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public NavbarModel Navbar { get; set; } = new NavbarModel();
        public Product? Product { get; set; }
        public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
        public Cart? Cart { get; set; }
        public Order? Orders { get; set; }
        public string? ErrorMessage { get; set; } = string.Empty;
        public List<Product> LatestProducts { get; set; } = new List<Product>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public UserModel User { get; set; } = new UserModel();
        public Guid UserId { get; set; }
        public Address? Address { get; set; }

    }
}
