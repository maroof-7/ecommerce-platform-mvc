using Microsoft.AspNetCore.Mvc;
using DummyProject.Data;
using DummyProject.Models.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DummyProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly SqlDbContext _context;

        public OrderController(SqlDbContext context)
        {
            _context = context;
        }

        // GET: Orders (Admin sees all orders)
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Buyer) // Make sure 'Buyer' is the navigation property to User
                .OrderByDescending(o => o.DateCreated)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> DispatchOrder(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status != "Cancelled")
            {
                order.Status = "Dispatched";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyOrders");
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status != "Dispatched")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyOrders");
        }

        // GET: Order Details
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("MyOrders");
        }

        [AllowAnonymous]
        public IActionResult OrderSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Payment(Guid OrderId, int OrderValue)
        {
            ViewData["OrderId"] = OrderId;
            ViewData["OrderValue"] = OrderValue;
            return View();
        }
    }
}