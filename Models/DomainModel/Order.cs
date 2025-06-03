using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DummyProject.Models.JunctionModels;

namespace DummyProject.Models.DomainModel
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();

        public Guid BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }  
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal OrderTotal { get; set; }

        public decimal TotalAmount { get; set; } // Added this for total price calculation

        public string Status { get; set; } = "Pending"; // Possible values: Pending, Completed, Cancelled

        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
