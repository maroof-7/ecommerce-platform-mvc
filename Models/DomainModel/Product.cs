using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DummyProject.Models.JunctionModels;

namespace DummyProject.Models.DomainModel
{
    public class Product
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Product title is required.")]
        [StringLength(100, ErrorMessage = "Product title cannot exceed 100 characters.")]
        public required string ProductTitle { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "Product image URL is required.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        [Display(Name = "Product Image")]
        public required string ProductPicURl { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        public required int Qty { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
        public required int Price { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public required string Category { get; set; }

        [Required(ErrorMessage = "Subcategory is required.")]
        [StringLength(50, ErrorMessage = "Subcategory cannot exceed 50 characters.")]
        public required string SubCategory { get; set; }

        [Required(ErrorMessage = "Hash tags are required.")]
        [StringLength(200, ErrorMessage = "Hash tags cannot exceed 200 characters.")]
        public required string HashTags { get; set; }

        [Required(ErrorMessage = "Seller ID is required.")]
        public required Guid SellerId { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        public ICollection<CartProduct> Carts { get; set; } = [];

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; internal set; }
        public int StockQuantity { get; set; }

        [NotMapped]
        public string? ImageUrl => ProductPicURl;
    }
}