using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DummyProject.Models.JunctionModels;
using DummyProject.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DummyProject.Models.DomainModel
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public PaymentMethod PaymentMethod { get; set; }
        public decimal OrderTotal { get; set; }
        public Guid BuyerId { get; set; }

        [ForeignKey("BuyerId")]

        public User? Buyer { get; set; }

        public Guid AddressId { get; set; }

        [ForeignKey("AddressId")]

        public Address? Address { get; set; }
        public DateTime DateCreated { get; set; }

        public decimal TotalAmount { get; set; } // Added this for total price calculation

        public string Status { get; set; } = "Pending"; // Possible values: Pending, Completed, Cancelled

        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();


    }
}
