using Microsoft.AspNetCore.Mvc;

namespace SurveyWebsite.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
