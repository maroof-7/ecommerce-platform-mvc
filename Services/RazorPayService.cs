// using Razorpay.Api; // Ensure Razorpay NuGet package is installed
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DummyProject.Interfaces;
// using Microsoft.Extensions.Configuration;

// namespace DummyProject.Services
// {
//     public class RazorpayService : IRazorpayService
//     {
//         private readonly RazorpayClient _client;
//         private readonly IConfiguration _configuration;

//         public RazorpayService(IConfiguration configuration)
//         {
//             _configuration = configuration;

//             // Initialize Razorpay client with Key ID and Secret
//             var keyId = _configuration["Razorpay:KeyId"];
//             var keySecret = _configuration["Razorpay:KeySecret"];

//             if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
//             {
//                 throw new ArgumentNullException("Razorpay KeyId or KeySecret is missing in configuration.");
//             }

//             _client = new RazorpayClient(keyId, keySecret);
//         }

//         public async Task<PaymentOrder> CreateOrderAsync(decimal amount, string currency, string? receipt = null)
//         {
//             try
//             {
//                 var options = new Dictionary<string, object>
//                 {
//                     { "amount", (int)(amount * 100) }, // Convert to smallest currency unit (e.g., paise for INR)
//                     { "currency", currency },
//                     { "receipt", receipt ?? Guid.NewGuid().ToString() },
//                     { "payment_capture", 1 } // Auto-capture payment
//                 };

//                 var order = await Task.Run(() => _client.Order.Create(options));
//                 return order;
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception("Failed to create Razorpay order.", ex);
//             }
//         }

//         public bool VerifyPayment(string paymentId, string orderId, string signature)
//         {
//             var attributes = new Dictionary<string, string>
//             {
//                 { "razorpay_payment_id", paymentId },
//                 { "razorpay_order_id", orderId },
//                 { "razorpay_signature", signature }
//             };

//             Utils.ValidatePaymentSignature(attributes);
//             return true; // If no exception is thrown, signature is valid
//         }

//         public async Task<Payment> FetchPaymentAsync(string paymentId)
//         {
//             try
//             {
//                 var payment = await Task.Run(() => _client.Payment.Fetch(paymentId));
//                 return payment;
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception("Failed to fetch payment.", ex);
//             }
//         }

//         public async Task RefundPaymentAsync(string paymentId, decimal amount, string? notes = null)
//         {
//             try
//             {
//                 var options = new Dictionary<string, object>
//                 {
//                     { "amount", (int)(amount * 100) }, // Convert to smallest currency unit
//                     { "notes", notes }
//                 };

//                 await Task.Run(() => _client.Payment.Refund(paymentId, options));
//             }
//             catch (Exception ex)
//             {
//                 throw new Exception("Failed to refund payment.", ex);
//             }
//         }
//     }
// }