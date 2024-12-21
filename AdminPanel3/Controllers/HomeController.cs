using AdminPanel3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminPanel3.Controllers
{
    [CheckAccess]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Alerts()
        {
            return View();
        }

        public IActionResult Accordion()
        {
            return View();
        }

        public IActionResult Badges()
        {
            return View();
        }

        public IActionResult BreadCrumbs()
        {
            return View();
        }

        public IActionResult FormElements()
        {
            return View();
        }

        public IActionResult FormLayouts()
        {
            return View();
        }

        public IActionResult FormEditors()
        {
            return View();
        }

        public IActionResult FormValidation()
        {
            return View();
        }

        public IActionResult GeneralTables()
        {
            return View();
        }

        public IActionResult DataTables()
        {
            return View();
        }

        public IActionResult ChartJs()
        {
            return View();
        }

        public IActionResult ApexCharts()
        {
            return View();
        }

        public IActionResult ECharts()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
