using Microsoft.AspNetCore.Mvc;

namespace DummyProject.Controllers
{
    public class NewsletterApi : Controller
    {
        // GET: NewsletterApi
        public ActionResult Index()
        {
            return View();
        }

    }
}
