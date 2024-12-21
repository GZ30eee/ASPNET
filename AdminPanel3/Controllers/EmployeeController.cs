using AdminPanel3.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel3.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult Form()
        {
            return View();
        }

    }
}
