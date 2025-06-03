using Microsoft.AspNetCore.Mvc;
using DummyProject.Data;
using DummyProject.Models.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DummyProject.Models.JunctionModels;

namespace DummyProject.Controllers
{
    public class OrderController : Controller
    {
        private readonly SqlDbContext _context;

        public OrderController(SqlDbContext context)
        {
            _context = context;
        }

        // GET: Orders (List of Orders for a User)
        public async Task<IActionResult> Index()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = Guid.Parse(token); // Replace with actual ID retrieval logic

            var orders = await _context.Orders
                .Where(o => o.BuyerId == userId)
                .ToListAsync(); // Removed .Include(o => o.Buyer) as Buyer navigation is missing

            return View(orders);
        }

        // GET: Order Details
        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts) // Include ordered products
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Place Order (From Cart)
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = Guid.Parse(token); // Replace with actual ID retrieval logic

            var cart = await _context.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.BuyerId == userId);

            if (cart == null || !cart.CartProducts.Any())
            {
                return BadRequest("Your cart is empty.");
            }

            // Create new order
            var order = new Order
            {
                BuyerId = userId,
                TotalAmount = cart.CartProducts.Sum(cp => cp.Product.Price * cp.Quantity),
                Status = "Pending",
                OrderProducts = cart.CartProducts.Select(cp => new OrderProduct
                {
                    ProductId = cp.ProductId,
                    Quantity = cp.Quantity,
                    Price = cp.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // Remove items from cart after placing the order
            _context.CartProducts.RemoveRange(cart.CartProducts);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: Update Order Status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status; // Changed from OrderStatus to Status
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
