using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DummyProject.Data;
using DummyProject.Interfaces;
using DummyProject.Types;
using DummyProject.Models;
using DummyProject.Models.ViewModel;
using DummyProject.Models.JunctionModels;
using DummyProject.Models.DomainModel;
using Microsoft.IdentityModel.Tokens;

namespace DummyProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly ITokenService tokenService;

        public AdminController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        // GET: AdminController
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            // Retrieve the token from the cookie
            var token = Request.Cookies["authToken"];

            // If the token is missing, redirect to the login page
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            try
            {
                // Verify the token and get the user ID
                var userId = tokenService.VerifyTokenAndGetId(token);

                // Fetch the user from the database
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));

                // Check if the user is an admin
                if (user?.Role == Role.Admin)
                {
                    // If the user is an admin, return the admin dashboard view
                    return View();
                }
            }
            catch (SecurityTokenExpiredException)
            {
                // If the token is expired, redirect to the login page
                return RedirectToAction("Login", "User");
            }
            catch (SecurityTokenValidationException)
            {
                // If the token is invalid, redirect to the login page
                return RedirectToAction("Login", "User");
            }

            // If the user is not an admin or the token is invalid, redirect to the login page
            return RedirectToAction("Login", "User");
        }
        [HttpGet]
        public IActionResult GetSalesData()
        {
            // Replace this with your actual data fetching logic (e.g., from a database)
            var salesData = new
            {
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                Data = new[] { 120, 200, 150, 300, 250, 400, 350, 500, 450, 600, 550, 700 } // Example data
            };
            return Json(salesData);
        }

        // GET: AdminController/CreateProduct
        [HttpGet]
        public IActionResult CreateProduct()
        {
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        // POST: AdminController/CreateProduct
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = tokenService.VerifyTokenAndGetId(token);
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserId == Guid.Parse(userId));

            if (user?.Role == Role.Admin)
            {
                product.SellerId = user.UserId;
                await dbContext.Products.AddAsync(product);
                await dbContext.SaveChangesAsync();
                return RedirectToAction("AdminDashboard");
            }

            return RedirectToAction("Login", "User");
        }




        // GET: AdminController/MyProducts
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = tokenService.VerifyTokenAndGetId(token); // logged in userId

            // Fetch products for the logged-in user
            var myProducts = await dbContext.Products
                .Where(p => p.SellerId == Guid.Parse(userId) && !p.IsArchived && !p.IsDeleted)
                .ToListAsync();

            myProducts ??= new List<Product>(); // Ensure it's not null

            // Create a HybridViewModel and populate it
            var viewModel = new HybridViewModel
            {
                Products = myProducts,
                Navbar = new NavbarModel { IsLoggedin = true, UserRole = Role.Admin, UserId = Guid.Parse(userId) }, // Populate dynamically
                Product = null, // Set this if you need to pass a single product
                CartProducts = new List<CartProduct>() // Populate this if needed
            };

            return View(viewModel);
        }
        // GET: AdminController/MyArchive
        [HttpGet]
        public async Task<IActionResult> MyArchive()
        {
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = tokenService.VerifyTokenAndGetId(token); // logged in userId

            var myProducts = await dbContext.Products
                .Where(p => p.SellerId == Guid.Parse(userId) && p.IsArchived && !p.IsDeleted)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = myProducts
            };

            return View(viewModel);
        }

        // GET: AdminController/DeletedProducts
        [HttpGet]
        public async Task<IActionResult> DeletedProducts()
        {
            var token = Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "User");
            }

            var userId = tokenService.VerifyTokenAndGetId(token); // logged in userId

            var myProducts = await dbContext.Products
                .Where(p => p.SellerId == Guid.Parse(userId) && p.IsDeleted && p.IsArchived)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = myProducts
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> FeaturedProducts()
        {
            // Fetch featured products from the database
            var featuredProducts = await dbContext.Products
                .Where(p => p.IsFeatured && !p.IsDeleted) // Only fetch featured and non-deleted products
                .ToListAsync();

            // Create the view model
            var viewModel = new HybridViewModel
            {
                Products = featuredProducts // Pass the featured products to the view
            };

            // Return the view with the featured products
            return View(viewModel);
        }




        // POST: AdminController/ArchiveProduct
        [HttpPost]
        public async Task<IActionResult> ArchiveProduct(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsArchived = true;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyProducts");
        }

        // POST: AdminController/UnArchiveProduct
        [HttpGet]
        public async Task<IActionResult> UnArchiveProduct(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsArchived = false;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyArchive");
        }




        [HttpGet]
        public async Task<IActionResult> SendToRecycle(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsDeleted = true;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyArchive");
        }


        [HttpGet]
        public async Task<IActionResult> RestoreFromRecycle(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsDeleted = false;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("DeletedProducts");
        }



        [HttpPost]
        public async Task<IActionResult> MarkAsFeatured(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsFeatured = true;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        public async Task<IActionResult> UnmarkAsFeatured(Guid id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product != null)
            {
                product.IsFeatured = false;
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("MyArchive");
        }
    }
}