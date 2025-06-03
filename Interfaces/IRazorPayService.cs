// using Razorpay.Api;
// using System;
// using System.Threading.Tasks;

// namespace DummyProject.Interfaces
// {
//     public interface IRazorpayService
//     {
//         Task<PaymentOrder> CreateOrderAsync(decimal amount, string currency, string? receipt = null);
//         bool VerifyPayment(string paymentId, string orderId, string signature);
//         Task<Payment> FetchPaymentAsync(string paymentId);
//         Task RefundPaymentAsync(string paymentId, decimal amount, string? notes = null);
//     }
// }