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

        // GET: AdminController/AdminDashboard
        [HttpGet]
        public async Task<IActionResult> AdminDashboard()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

            try
            {
                var userId = tokenService.VerifyTokenAndGetId(token);
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));
                if (user?.Role == Role.Admin)
                {
                    var stats = new AdminDashboardStatsViewModel
                    {
                        TotalOrders = await dbContext.Orders.CountAsync(),
                        TotalRevenue = await dbContext.Orders.SumAsync(o => (decimal?)o.OrderTotal) ?? 0,
                        TotalCustomers = await dbContext.Users.CountAsync(u => u.Role == Role.Buyer),
                        LowStockProducts = await dbContext.Products.CountAsync(p => p.StockQuantity <= 5)
                    };
                    return View(stats);
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return RedirectToAction("Login", "User");
            }
            catch (SecurityTokenValidationException)
            {
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Login", "User");
        }
        [HttpGet]
        public IActionResult GetSalesData()
        {
            var salesData = new
            {
                Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
                Data = new[] { 120, 200, 150, 300, 250, 400, 350, 500, 450, 600, 550, 700 }
            };
            return Json(salesData);
        }

        // GET: AdminController/CreateProduct
        [HttpGet]
        public IActionResult CreateProduct()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");
            return View();
        }

        // POST: AdminController/CreateProduct
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

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

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        // GET: AdminController/OrderList
        [HttpGet]
        public async Task<IActionResult> OrderList()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

            try
            {
                var userId = tokenService.VerifyTokenAndGetId(token);
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));
                if (user?.Role == Role.Admin)
                {
                    var orders = await dbContext.Orders
                        .Include(o => o.Buyer)
                        .Include(o => o.Address)
                        .OrderByDescending(o => o.OrderDate)
                        .ToListAsync();
                    return View(orders); // Views/Admin/OrderList.cshtml
                }
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Login", "User");
        }

        // GET: AdminController/ProductList
        [HttpGet]
        public async Task<IActionResult> ProductList()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

            try
            {
                var userId = tokenService.VerifyTokenAndGetId(token);
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));
                if (user?.Role == Role.Admin)
                {
                    var products = await dbContext.Products
                        .OrderByDescending(p => p.DateCreated)
                        .ToListAsync();

                    var viewModel = new HybridViewModel
                    {
                        Products = products
                        // Fill other properties if needed
                    };

                    return View(viewModel); // Views/Admin/ProductList.cshtml
                }
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Login", "User");
        } // GET: AdminController/UserList
        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            var users = await dbContext.Users.ToListAsync();
            return View(users);
        }

        // GET: AdminController/MyProducts
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

            try
            {
                var userId = tokenService.VerifyTokenAndGetId(token);
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId));
                if (user?.Role == Role.Admin)
                {
                    var products = await dbContext.Products
                        .OrderByDescending(p => p.DateCreated)
                        .ToListAsync();

                    var viewModel = new HybridViewModel
                    {
                        Products = products
                        // Fill other properties if needed
                    };

                    return View(viewModel); // Views/Admin/ProductList.cshtml
                }
            }
            catch
            {
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Login", "User");
        } // GET: AdminController/MyArchive
        [HttpGet]
        public async Task<IActionResult> MyArchive()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "User");

            var userId = tokenService.VerifyTokenAndGetId(token);
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
                return RedirectToAction("Login", "User");

            var userId = tokenService.VerifyTokenAndGetId(token);
            var myProducts = await dbContext.Products
                .Where(p => p.SellerId == Guid.Parse(userId) && p.IsDeleted && p.IsArchived)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = myProducts
            };

            return View(viewModel);
        }

        // GET: AdminController/FeaturedProducts
        [HttpGet]
        public async Task<IActionResult> FeaturedProducts()
        {
            var featuredProducts = await dbContext.Products
                .Where(p => p.IsFeatured && !p.IsDeleted)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = featuredProducts
            };

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
            return RedirectToAction("MyArchive");
        }

        // GET: AdminController/UnArchiveProduct
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

        // GET: AdminController/SendToRecycle
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

        // GET: AdminController/RestoreFromRecycle
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

        // POST: AdminController/MarkAsFeatured
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

        // POST: AdminController/UnmarkAsFeatured
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
        [HttpGet]
        public async Task<IActionResult> GetSalesData(string period = "week")
        {
            // Example: You should replace this with your actual sales aggregation logic
            List<string> labels;
            List<decimal> data;

            if (period == "month")
            {
                labels = Enumerable.Range(1, 12).Select(i => System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i)).ToList();
                data = await dbContext.Orders
                    .GroupBy(o => o.OrderDate.Month)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Sum(o => o.OrderTotal))
                    .ToListAsync();
            }
            else if (period == "year")
            {
                int currentYear = DateTime.Now.Year;
                labels = Enumerable.Range(currentYear - 4, 5).Select(y => y.ToString()).ToList();
                data = await dbContext.Orders
                    .Where(o => o.OrderDate.Year >= currentYear - 4)
                    .GroupBy(o => o.OrderDate.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Sum(o => o.OrderTotal))
                    .ToListAsync();
            }
            else // week
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                labels = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i).ToString("ddd")).ToList();
                data = new List<decimal>();
                for (int i = 0; i < 7; i++)
                {
                    var day = startOfWeek.AddDays(i).Date;
                    var nextDay = day.AddDays(1);
                    var sum = await dbContext.Orders
                        .Where(o => o.OrderDate >= day && o.OrderDate < nextDay)
                        .SumAsync(o => (decimal?)o.OrderTotal) ?? 0;
                    data.Add(sum);
                }
            }

            return Json(new { labels, data });
        }
    }
}