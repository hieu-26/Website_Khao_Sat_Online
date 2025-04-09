using Microsoft.AspNetCore.Mvc;

namespace SurveyWebsite.Controllers
{
    public class SurveyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
