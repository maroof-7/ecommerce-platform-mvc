using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DummyProject.Models.ViewModel;
using DummyProject.Data;
using DummyProject.Interfaces;
using Microsoft.EntityFrameworkCore;
using DummyProject.Models.DomainModel;
using DummyProject.Models;
using DummyProject.Types;

namespace DummyProject.Controllers;

public class HomeController : Controller
{
    private readonly SqlDbContext dbContext;
    private readonly ITokenService tokenService;
    private readonly ILogger<HomeController> logger;

    public HomeController(SqlDbContext dbContext, ITokenService tokenService, ILogger<HomeController> logger)
    {
        this.dbContext = dbContext;
        this.tokenService = tokenService;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {

        // Fetch featured and latest products
        var featuredProducts = await dbContext.Products
            .Where(p => p.IsFeatured && !p.IsDeleted)
            .ToListAsync();

        var latestProducts = await dbContext.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.DateCreated)
            .Take(4)
            .ToListAsync();


        // Create the view model
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false, UserRole = Role.Buyer, UserId = Guid.Empty }, // Default values
            FeaturedProducts = featuredProducts,
            LatestProducts = latestProducts
        };

        return View(viewModel);


    }
    [HttpGet]
    public async Task<IActionResult> Shop()
    {
        try
        {
            var products = await dbContext.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Navbar = new NavbarModel { IsLoggedin = false }, // Hardcoded values (update dynamically if needed)
                Products = products
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in HomeController Shop method.");
            return View("Error");
        }
    }

    [HttpGet]
    public IActionResult About()
    {
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false } // Hardcoded values (update dynamically if needed)
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Services()
    {
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false } // Hardcoded values (update dynamically if needed)
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Blog()
    {
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false } // Hardcoded values (update dynamically if needed)
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Contact()
    {
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false } // Hardcoded values (update dynamically if needed)
        };
        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false } // Hardcoded values (update dynamically if needed)
        };
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()

    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [HttpGet]
    public async Task<IActionResult> Electronics()
    {
        var products = await dbContext.Products
            .Where(p => p.Category == "Electronics" && !p.IsDeleted)
            .ToListAsync();

        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false },
            Products = products
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Fashion()
    {
        var products = await dbContext.Products
            .Where(p => p.Category == "Fashion" && !p.IsDeleted)
            .ToListAsync();

        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false },
            Products = products
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> HomeDecor()
    {
        var products = await dbContext.Products
            .Where(p => p.Category == "HomeDecor" && !p.IsDeleted)
            .ToListAsync();

        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false },
            Products = products
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SportsFitness()
    {
        var products = await dbContext.Products
            .Where(p => p.Category == "SportsFitness" && !p.IsDeleted)
            .ToListAsync();

        var viewModel = new HybridViewModel
        {
            Navbar = new NavbarModel { IsLoggedin = false },
            Products = products
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            // If the search query is empty, return all products or a message
            var allProducts = await dbContext.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var viewModel = new HybridViewModel
            {
                Products = allProducts
            };

            return View(viewModel);
        }

        // Perform the search based on the query
        var searchResults = await dbContext.Products
            .Where(p => !p.IsDeleted &&
                        (p.ProductTitle.Contains(query) ||
                         p.Description.Contains(query) ||
                         p.Category.Contains(query) ||
                         p.SubCategory.Contains(query)))
            .ToListAsync();

        var searchViewModel = new HybridViewModel
        {
            Products = searchResults
        };

        return View("Search", searchViewModel);
    }


    // Customer Service Pages
    [HttpGet]
    public IActionResult ReturnPolicy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult TrackReturn()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ManageSubscription()
    {
        return View();
    }

    // Our Company Pages
    [HttpGet]
    public IActionResult AboutUs()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SuperStore()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Careers()
    {
        return View();
    }

    // Explore More Pages
    [HttpGet]
    public IActionResult FederalMarketplace()
    {
        return View();
    }

    [HttpGet]
    public IActionResult StudentAdvantage()
    {
        return View();
    }

    [HttpGet]
    public IActionResult PhotographyPodcast()
    {
        return View();
    }

    
}
