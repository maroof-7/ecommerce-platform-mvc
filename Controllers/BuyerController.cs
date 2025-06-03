using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DummyProject.Data;
using DummyProject.Interfaces;
using DummyProject.Models;
using DummyProject.Models.JunctionModels;
using DummyProject.Models.ViewModel;
using DummyProject.Types;
using DummyProject.Models.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DummyProject.ViewModels;

namespace DummyProject.Controllers
{
    public class BuyerController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly ITokenService tokenService;
        private readonly HybridViewModel viewModel;

        public BuyerController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.viewModel = new HybridViewModel
            {
                Navbar = new NavbarModel { UserRole = Role.Buyer, IsLoggedin = true }
            };
        }



        [HttpGet]
        public IActionResult Dashboard()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "User");
                }

                var userId = tokenService.VerifyTokenAndGetId(token);
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid buyerId))
                {
                    var errorViewModel = new ErrorViewModel
                    {
                        ErrorMessage = "Invalid or expired token!",
                        ShowRequestId = false
                    };
                    return View("Error", errorViewModel);
                }

                var cart = await dbContext.Carts
                    .Include(c => c.CartProducts)
                    .ThenInclude(cp => cp.Product)
                    .FirstOrDefaultAsync(c => c.BuyerId == buyerId);

                if (cart == null)
                {
                    ViewBag.CartEmpty = "Cart is Empty";
                    return View(viewModel);
                }

                viewModel.CartProducts = cart.CartProducts?.ToList() ?? new List<CartProduct>();
                viewModel.Cart = cart;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = $"An error occurred: {ex.Message}",
                    ShowRequestId = false
                };
                return View("Error", errorViewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress(Address address)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = tokenService.VerifyTokenAndGetId(token);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid buyerId))
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = "Invalid or expired token!",
                    ShowRequestId = false
                };
                return View("Error", errorViewModel);
            }

            var existingAddress = await dbContext.Addresses.FirstOrDefaultAsync(a => a.BuyerId == buyerId);
            if (existingAddress != null)
            {
                ViewBag.ErrorMessage = "Address already exists!";
                return View("CheckOut", viewModel);
            }

            if (ModelState.IsValid)
            {
                address.BuyerId = buyerId;
                await dbContext.Addresses.AddAsync(address);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("CheckOut");
            }

            ViewBag.ErrorMessage = "Address update unsuccessful!";
            return View("CheckOut", viewModel);
        }

        [HttpGet]
        public IActionResult CheckOut(Guid CartId)
        {

            return View(viewModel);
        }



        [HttpPost]
        public IActionResult Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return view with validation errors
            }

            var cart = dbContext.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefault(c => c.BuyerId == model.UserId); // Fixed reference issue

            if (cart == null || !cart.CartProducts.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }

            // Create a new order
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                BuyerId = model.UserId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                OrderTotal = model.TotalAmount,
                OrderProducts = cart.CartProducts.Select(cp => new OrderProduct
                {
                    ProductId = cp.Product.ProductId,
                    Quantity = cp.Quantity,
                    Price = cp.Product.Price
                }).ToList()
            };

            dbContext.Orders.Add(order);
            dbContext.CartProducts.RemoveRange(cart.CartProducts); // Clear the cart after checkout
            dbContext.SaveChanges();

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("MyOrders");
        }

    }
}



