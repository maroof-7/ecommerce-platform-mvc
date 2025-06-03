using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DummyProject.Data;
using System.IdentityModel.Tokens.Jwt;
using DummyProject.Interfaces;
using DummyProject.Types;
using DummyProject.Models.DomainModel;
using DummyProject.Models;


namespace DummyProject.Controllers
{
    public class UserController : Controller
    {
        private readonly SqlDbContext dbContext;
        private readonly ITokenService tokenService;

        public UserController(SqlDbContext dbContext, ITokenService tokenService)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            var model = new RegisterModel { ReturnUrl = returnUrl ?? Url.Action("Index", "Home") ?? "/" };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "All fields are required!";
                return View(model);
            }

            try
            {
                // Ensure dbContext is not null (avoids null reference error)
                if (dbContext.Users == null)
                {
                    ViewData["ErrorMessage"] = "Database connection error.";
                    return View(model);
                }

                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ViewData["ErrorMessage"] = "User already exists.";
                    return View(model);
                }

                var newUser = new User
                {
                    Email = model.Email ?? throw new ArgumentNullException(nameof(model.Email)),
                    Password = model.Password != null ? BCrypt.Net.BCrypt.HashPassword(model.Password) : throw new ArgumentNullException(nameof(model.Password)),
                    FirstName = model.FirstName ?? "",
                    LastName = model.LastName ?? "",
                    PhoneNumber = model.PhoneNumber ?? "",
                    Role = Role.Buyer,// Default role
                    Username = model.Username ?? ""

                };

                await dbContext.Users.AddAsync(newUser);
                await dbContext.SaveChangesAsync();

                ViewData["SuccessMessage"] = "User created successfully!";
                // return RedirectToLocal(model.ReturnUrl);
                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            try
            {

                var findUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (findUser == null)
                {
                    ViewData["ErrorMessgae"] = "User not Found";
                    return View(model);
                }

                if (!BCrypt.Net.BCrypt.Verify(model.Password, findUser.Password))
                {
                    ViewData["ErrorMessage"] = "Invalid email or password.";
                    return View();
                }

                var token = tokenService.CreateToken(findUser.UserId.ToString(), findUser.Email, findUser.Username);

                HttpContext.Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(24)
                });

                // ViewData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");

                // return findUser.Role switch
                // {
                //     Role.Admin => RedirectToAction("AdminDashboard"),
                //     Role.Seller => RedirectToAction("SellerDashboard"),
                //     _ => RedirectToAction("BuyerDashboard")
                // };
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return View();
            }
        }


        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
