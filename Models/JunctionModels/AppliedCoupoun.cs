// In Models/DomainModel/Coupon.cs
using DummyProject.Models.DomainModel;

public class Coupon
{
    public Guid CouponId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

// In Models/JunctionModels/AppliedCoupon.cs
public class AppliedCoupon
{
    public Guid AppliedCouponId { get; set; }
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }
    public Guid CouponId { get; set; }
    public Coupon Coupon { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
}