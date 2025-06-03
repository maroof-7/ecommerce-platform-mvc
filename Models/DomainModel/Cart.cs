using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DummyProject.Models.JunctionModels;

namespace DummyProject.Models.DomainModel
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; } = Guid.NewGuid();

        public Guid BuyerId { get; set; } // Every Cart belongs to a specific Buyer
        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        public int CartValue { get; set; }

        // Navigation property for CartProducts
        public ICollection<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

        
    }
}