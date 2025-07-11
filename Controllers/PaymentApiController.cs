using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DummyProject.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        private readonly RazorpayService razorpayService = new RazorpayService();




        [HttpPost("create/paymentIntent")]
        public IActionResult CreatePaymentIntent([FromBody] CreatePaymentIntentModel model)
        {
            if (model == null || model.Amount <= 0 || string.IsNullOrEmpty(model.Currency))
            {
                return BadRequest("Invalid payment details.");
            }
            try
            {
                var order = razorpayService.CreateOrder(model.Amount, model.Currency, model.OrderId);

                if (order == null)
                {
                    return StatusCode(500, "Failed to create payment order.");
                }

                return Ok(new
                {
                    orderId = order["id"].ToString(),
                    entity = order["entity"].ToString(),
                    amount = order["amount"],
                    amountPaid = order["amount_paid"],
                    amountDue = order["amount_due"],
                    currency = order["currency"].ToString(),
                    receipt = order["receipt"].ToString(),
                    status = order["status"].ToString(),
                    attempts = order["attempts"],
                    createdAt = order["created_at"]
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        public class CreatePaymentIntentModel
        {
            public int Amount { get; set; }
            public string? Currency { get; set; }
            public Guid OrderId { get; set; }
        }

    }
}
