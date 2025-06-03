using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DummyProject.Data;
using DummyProject.Interfaces;
using DummyProject.Models;
using DummyProject.Models.JunctionModels;
using DummyProject.Models.ViewModel;
using DummyProject.Models.DomainModel;
using DummyProject.Types;
using Microsoft.Extensions.Logging;

namespace DummyProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly ITokenService tokenService;
        private readonly HybridViewModel viewModel;
        private readonly ILogger<AdminController> _logger; // Define the logger

        

        public ProductController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.viewModel = new HybridViewModel
            {
                Navbar = new NavbarModel { IsLoggedin = false }, // Hardcoded values (update dynamically if needed)
                Products = []
            };
        }

        // Display product details
        [HttpGet]
        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                var products = await dbContext.Products
                    .Where(p => p.Category == product.Category && p.SubCategory == product.SubCategory)
                    .ToListAsync();

                viewModel.Product = product;
                viewModel.Products = products;

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

        // Add a product to the cart
        [HttpGet]
        public async Task<ActionResult> AddToCart(Guid ProductId)
        {
            try
            {
                var token = Request.Cookies["AuthToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "User");
                }

                var userId = tokenService.VerifyTokenAndGetId(token); // LOGGED IN USERID
                var product = await dbContext.Products.FindAsync(ProductId);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (Guid.Parse(userId) == product.SellerId)
                {
                    var errorViewModel = new ErrorViewModel
                    {
                        ErrorMessage = "Can't buy your own product!",
                        ShowRequestId = false
                    };
                    return View("Error", errorViewModel);
                }

                var cart = await dbContext.Carts
                    .FirstOrDefaultAsync(c => c.BuyerId == Guid.Parse(userId)); // Find the cart

                if (cart == null)
                {
                    cart = new Cart
                    {
                        BuyerId = Guid.Parse(userId),
                        CartValue = 0
                    };
                    await dbContext.Carts.AddAsync(cart);
                    await dbContext.SaveChangesAsync();
                }

                var existingCartProduct = await dbContext.CartProducts
                    .FirstOrDefaultAsync(cp => cp.CartId == cart.CartId && cp.ProductId == ProductId); // Find the cart product

                if (existingCartProduct == null)
                {
                    var cartProduct = new CartProduct
                    {
                        CartId = cart.CartId,
                        ProductId = ProductId,
                        Quantity = 1
                    };

                    await dbContext.CartProducts.AddAsync(cartProduct);
                    cart.CartValue += product.Price;
                }
                else
                {
                    existingCartProduct.Quantity += 1;
                    cart.CartValue += product.Price;
                }

                await dbContext.SaveChangesAsync();
                return RedirectToAction("Cart", "Buyer");
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

        // Soft delete a product
        [HttpPost]
        public async Task<IActionResult> Delete(Guid productId)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Soft delete the product
                product.IsDeleted = true;
                product.DateModified = DateTime.UtcNow; // Update the modification date
                dbContext.Products.Update(product);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("MyProducts");
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

        // Restore a soft-deleted product
        [HttpPost]
        public async Task<IActionResult> Restore(Guid productId)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Restore the product
                product.IsDeleted = false;
                product.DateModified = DateTime.UtcNow; // Update the modification date
                dbContext.Products.Update(product);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("MyProducts");
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

        // Archive a product
        [HttpPost]
        public async Task<IActionResult> ArchiveProduct(Guid productId)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Archive the product
                product.IsArchived = true;
                product.DateModified = DateTime.UtcNow; // Update the modification date
                dbContext.Products.Update(product);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("MyProducts");
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

        // Unarchive a product
        [HttpPost]
        public async Task<IActionResult> UnarchiveProduct(Guid productId)
        {
            try
            {
                var product = await dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Unarchive the product
                product.IsArchived = false;
                product.DateModified = DateTime.UtcNow; // Update the modification date
                dbContext.Products.Update(product);
                await dbContext.SaveChangesAsync();

                return RedirectToAction("MyProducts");
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

        // List active products
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var activeProducts = await dbContext.Products
                .Where(p => !p.IsDeleted && !p.IsArchived) // Exclude soft-deleted and archived products
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = activeProducts,
                Navbar = new NavbarModel { IsLoggedin = true } // Update as needed
            };

            return View(viewModel);
        }

        // List archived products
        [HttpGet]
        public async Task<IActionResult> ArchivedProducts()
        {
            var archivedProducts = await dbContext.Products
                .Where(p => p.IsArchived && !p.IsDeleted) // Show only archived and not deleted products
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = archivedProducts,
                Navbar = new NavbarModel { IsLoggedin = true } // Update as needed
            };

            return View("MyProducts", viewModel); // Reuse the same view
        }

        // List deleted products
        [HttpGet]
        public async Task<IActionResult> DeletedProducts()
        {
            try
            {
                // Fetch soft-deleted products
                var deletedProducts = await dbContext.Products
                    .Where(p => p.IsDeleted) // Only include soft-deleted products
                    .ToListAsync();

                // Create the view model
                var viewModel = new HybridViewModel
                {
                    Products = deletedProducts,
                    Navbar = new NavbarModel
                    {
                        IsLoggedin = User.Identity.IsAuthenticated // Dynamically set based on user authentication
                    }
                };

                // Return the correct view
                return View(viewModel); // Use the correct view name (DeletedProducts.cshtml)
            }
            catch (Exception ex)
            {
                // Log the error and return an error view
                // You can replace this with your preferred error handling mechanism
                _logger.LogError(ex, "An error occurred while fetching deleted products.");
                 return View("Error"); // Redirect to an error page
            }
        }
    }
    

}