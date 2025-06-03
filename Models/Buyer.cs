using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DummyProject.Models.DomainModel;

public class Buyer
{
    [Key]
    public Guid BuyerId { get; set; } = Guid.NewGuid();
    public string ?Name { get; set; }
    public string ?Email { get; set; }
    public List<Order> Orders { get; set; } = new List<Order>();
}
