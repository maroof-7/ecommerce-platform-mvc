
// using Microsoft.AspNetCore.Mvc;
// using DummyProject.Data;
// using DummyProject.Interfaces;
// using DummyProject.Services; // Assuming RazorpayService is here
// using Microsoft.Extensions.Logging;
// using System;
// using System.ComponentModel.DataAnnotations;
// using System.Threading.Tasks;

// namespace DummyProject.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class PaymentApiController : ControllerBase
//     {
//         private readonly SqlDbContext _dbContext;
//         private readonly ITokenService _tokenService;
//         private readonly IRazorpayService _razorpayService; // Use interface for DI
//         private readonly ILogger<PaymentApiController> _logger;

//         public PaymentApiController(
//             SqlDbContext dbContext,
//             ITokenService tokenService,
//             IRazorpayService razorpayService,
//             ILogger<PaymentApiController> logger)
//         {
//             _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
//             _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
//             _razorpayService = razorpayService ?? throw new ArgumentNullException(nameof(razorpayService));
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//         }

//         [HttpPost("create/paymentIntent")]
//         public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 _logger.LogWarning("Invalid payment details received.");
//                 return BadRequest(ModelState);
//             }

//             try
//             {
//                 var order = await _razorpayService.CreateOrderAsync(model.Amount, model.Currency, model.OrderId);

//                 if (order == null)
//                 {
//                     _logger.LogError("Failed to create Razorpay order.");
//                     return StatusCode(500, "Failed to create payment order.");
//                 }

//                 return Ok(new
//                 {
//                     orderId = order["id"].ToString(),
//                     entity = order["entity"].ToString(),
//                     amount = order["amount"],
//                     amountPaid = order["amount_paid"],
//                     amountDue = order["amount_due"],
//                     currency = order["currency"].ToString(),
//                     receipt = order["receipt"].ToString(),
//                     status = order["status"].ToString(),
//                     attempts = order["attempts"],
//                     createdAt = order["created_at"]
//                 });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error while creating payment intent.");
//                 return StatusCode(500, "Internal Server Error");
//             }
//         }

//         public class CreatePaymentIntentModel
//         {
//             [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
//             public int Amount { get; set; }

//             [Required(ErrorMessage = "Currency is required.")]
//             public string Currency { get; set; }

//             [Required(ErrorMessage = "OrderId is required.")]
//             public Guid OrderId { get; set; }
//         }
//     }
// }
