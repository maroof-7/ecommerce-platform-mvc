using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DummyProject.Models.DomainModel
{

    public class Address
    {
        [Key]
        public Guid AddressId { get; set; } = Guid.NewGuid();
        public required string FullName { get; set; }
        public required string EmailAddress { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Zipcode { get; set; }

        public required string Landmark { get; set; }

        public required string Phone { get; set; }
        public required string Country { get; set; }



        public required Guid BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateModified { get; set; } = DateTime.UtcNow;





    }
}