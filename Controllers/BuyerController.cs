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
using Microsoft.IdentityModel.Tokens;

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
            try
            {
                var userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var cart = await dbContext.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.BuyerId == Guid.Parse(userId));

            if (cart == null)
            {
                ViewBag.CartEmpty = "Cart is Empty";
                return View(viewModel);
            }

            viewModel.CartProducts = cart.CartProducts?.ToList() ?? new List<CartProduct>();
            viewModel.Cart = cart;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(Address address, Guid CartId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out Guid buyerId))
            {
                var errorViewModel = new ErrorViewModel
                {
                    ErrorMessage = "Invalid or expired token!",
                    ShowRequestId = false
                };
                return View("Error", errorViewModel);
            }

            var addressCount = await dbContext.Addresses.CountAsync(a => a.BuyerId == buyerId);
            if (addressCount >= 3)
            {
                TempData["ErrorMessage"] = "You can only add up to 3 addresses!";
                return RedirectToAction("CheckOut", new { CartId });
            }

            if (ModelState.IsValid)
            {
                address.BuyerId = buyerId;
                await dbContext.Addresses.AddAsync(address);
                await dbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Address added successfully!";
                return RedirectToAction("CheckOut", new { CartId });
            }

            TempData["ErrorMessage"] = "Address update unsuccessful!";
            return RedirectToAction("CheckOut", new { CartId });
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut(Guid CartId)
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

            // Always fetch fresh addresses from the database!
            var addresses = await dbContext.Addresses.Where(a => a.BuyerId == buyerId).ToListAsync();

            var cart = await dbContext.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.CartId == CartId && c.BuyerId == buyerId);

            if (cart == null || cart.CartProducts == null || !cart.CartProducts.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                CartId = cart.CartId,
                UserId = buyerId,
                CartItems = cart.CartProducts.Select(cp => new CartItemViewModel
                {
                    ProductId = cp.ProductId,
                    ProductName = cp.Product.ProductTitle,
                    ImageUrl = cp.Product.ProductPicURl,
                    Quantity = cp.Quantity,
                    Price = cp.Product.Price
                }).ToList(),
                TotalAmount = cart.CartValue,
                ShippingAddresses = addresses
            };

            return View(checkoutViewModel);
        }

        // --- FIX: Add the missing [HttpPost] Checkout action ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompletePurchase(Guid addressId, PaymentMethod paymentMethod)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }
            var userId = tokenService.VerifyTokenAndGetId(token);


            var cart = dbContext.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefault(c => c.BuyerId == Guid.Parse(userId));

            if (cart == null || cart.CartProducts == null || !cart.CartProducts.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }


            // Place the order directly for RazorPay
            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                BuyerId = Guid.Parse(userId),
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                OrderTotal = cart.CartValue,
                AddressId = addressId,
                PaymentMethod = paymentMethod, // This can remain a string if your Order model expects a string
                OrderProducts = cart.CartProducts.Select(cp => new OrderProduct
                {
                    ProductId = cp.Product.ProductId,
                    Quantity = cp.Quantity,
                    Price = cp.Product.Price
                }).ToList()
            };
            dbContext.Orders.Add(order);
            dbContext.CartProducts.RemoveRange(cart.CartProducts);
            // reset cart value 
            cart.CartValue = 0;
            dbContext.SaveChanges();

            if (paymentMethod == PaymentMethod.RazorPay)
            {
                return RedirectToAction("Payment", "Order", new { OrderId = order.OrderId, OrderValue = order.OrderTotal });
            }
            TempData["SuccessMessage"] = "Your Order has been Created!";

            return RedirectToAction("OrderSuccess", "Order");


        }



        private async Task UpdateCartValue(Guid cartId)
        {
            var cart = await dbContext.Carts
                .Include(c => c.CartProducts)
                .ThenInclude(cp => cp.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cart != null)
            {
                cart.CartValue = cart.CartProducts.Sum(cp => cp.Quantity * cp.Product.Price);
                await dbContext.SaveChangesAsync();
            }
        }

        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(Guid cartProductId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            var product = await dbContext.CartProducts
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.CartProductId == cartProductId);

            if (product == null) return NotFound();

            product.Quantity += 1;
            await dbContext.SaveChangesAsync();
            await UpdateCartValue(product.CartId);

            return RedirectToAction("Cart", "Buyer");
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(Guid cartProductId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            var product = await dbContext.CartProducts
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.CartProductId == cartProductId);

            if (product == null || product.Quantity <= 1) return BadRequest();

            product.Quantity -= 1;
            await dbContext.SaveChangesAsync();
            await UpdateCartValue(product.CartId);

            return RedirectToAction("Cart", "Buyer");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid cartProductId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            var product = await dbContext.CartProducts
                .Include(cp => cp.Product)
                .FirstOrDefaultAsync(cp => cp.CartProductId == cartProductId);

            if (product == null) return NotFound();

            var cartId = product.CartId;
            dbContext.CartProducts.Remove(product);
            await dbContext.SaveChangesAsync();
            await UpdateCartValue(cartId);

            return RedirectToAction("Cart", "Buyer");
        }

        [HttpPost]
        public IActionResult AddAddress(Address model, Guid CartId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            var addressCount = dbContext.Addresses.Count(a => a.BuyerId == Guid.Parse(userId));
            if (addressCount >= 3)
            {
                TempData["ErrorMessage"] = "You can only add up to 3 addresses!";
                return RedirectToAction("CheckOut", new { CartId });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Address details required!";
                return RedirectToAction("CheckOut", new { CartId });
            }

            model.BuyerId = Guid.Parse(userId);

            dbContext.Addresses.Add(model);
            dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Address added successfully!";
            return RedirectToAction("CheckOut", new { CartId });
        }

        [HttpGet]
        public IActionResult RemoveAddress(Guid addressId, Guid CartId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }


            var userId = tokenService.VerifyTokenAndGetId(token);

            var address = dbContext.Addresses.FirstOrDefault(a => a.AddressId == addressId && a.BuyerId == Guid.Parse(userId));

            if (address != null)
            {
                dbContext.Addresses.Remove(address);
                dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Address removed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Address not found!";
            }
            // Always redirect to CheckOut, which will fetch fresh addresses from DB
            return RedirectToAction("CheckOut", new { CartId });
        }

        [HttpPost]
        public IActionResult EditAddress(Address model, Guid addressId, Guid CartId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            string userId;
            try
            {
                userId = tokenService.VerifyTokenAndGetId(token);
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }

            var address = dbContext.Addresses.FirstOrDefault(a => a.AddressId == addressId && a.BuyerId == Guid.Parse(userId));
            if (address != null)
            {
                address.FullName = model.FullName;
                address.EmailAddress = model.EmailAddress;
                address.Phone = model.Phone;
                address.Landmark = model.Landmark;
                address.City = model.City;
                address.State = model.State;
                address.Zipcode = model.Zipcode;
                address.Country = model.Country;
                dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Address updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Address not found!";
            }
            return RedirectToAction("CheckOut", new { CartId });
        }


    }
}