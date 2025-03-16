using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagement.Api.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
