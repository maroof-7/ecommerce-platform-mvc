using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DummyProject.Models.DomainModel;

namespace DummyProject.ViewModels
{
    public class CheckoutViewModel
    {
        // Cart Information
        [Required(ErrorMessage = "Cart ID is required.")]
        public Guid CartId { get; set; }

        // User Information
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        // Address selection support
        public Guid? SelectedAddressId { get; set; }

        // Use List<Address> for index access in the view
        public List<Address> ShippingAddresses { get; set; } = new List<Address>();

        [Display(Name = "Applied Coupon")]
        public string? AppliedCouponCode { get; set; }

        [Display(Name = "Discount Amount")]
        [DataType(DataType.Currency)]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Is Coupon Applied")]
        public bool IsCouponApplied { get; set; }

        // For displaying multiple possible coupons
        public List<AppliedCouponInfo>? AvailableCoupons { get; set; }

        // Order Details
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Discount Applied")]
        [DataType(DataType.Currency)]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "Coupon Code")]
        public string? CouponCode { get; set; }

        [Display(Name = "Final Total")]
        [DataType(DataType.Currency)]
        public decimal FinalTotal => TotalAmount - Discount;

        // Payment Information
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        // Credit Card Fields (conditionally required)
        [Display(Name = "Card Number")]
        [CreditCard(ErrorMessage = "Invalid credit card number.")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Card number must be between 13-19 digits.")]
        public string? CardNumber { get; set; }

        [Display(Name = "Name on Card")]
        [StringLength(100, ErrorMessage = "Card name cannot exceed 100 characters.")]
        public string? CardName { get; set; }

        [Display(Name = "Expiration Date (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Invalid expiration date format.")]
        public string? CardExpiry { get; set; }

        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Invalid CVV (3-4 digits).")]
        public string? CardCvv { get; set; }

        // Payment Processing
        [JsonIgnore]
        public string? PaymentToken { get; set; }

        [Display(Name = "Payment Status")]
        public string? PaymentStatus { get; set; }

        // Additional Options
        [StringLength(500, ErrorMessage = "Order notes cannot exceed 500 characters.")]
        [Display(Name = "Special Instructions")]
        public string? OrderNotes { get; set; }

        // Timestamps
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Navigation properties for related data
        public List<AppliedCouponViewModel>? AppliedCoupons { get; set; }
    }

    public class CartItemViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public required string ProductName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal LineTotal => Quantity * Price;

        [Display(Name = "Product Image")]
        public string? ImageUrl { get; set; }
    }
    public class AppliedCouponInfo
    {
        public string? Code { get; set; }
        public decimal DiscountValue { get; set; }
        public string? DiscountType { get; set; } // "Percentage" or "Fixed"
    }

    public class AppliedCouponViewModel
    {
        public string? Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime AppliedDate { get; set; }
    }

    public enum PaymentMethod
    {


        CashOnDelivery,
        RazorPay,

    }
}