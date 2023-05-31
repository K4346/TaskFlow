using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskFlow.Models;

namespace TaskFlow.Controllers
{
    public class HomeController : Controller
    {


        private readonly ILogger<HomeController> _logger;


        private readonly TaskFlowContext db;

        public HomeController(ILogger<HomeController> logger, TaskFlowContext _db)
        {
            _logger = logger;
            db = _db;
        }

        public IActionResult Login()
        {


            return View();
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }

            List<Category> categories = db.Categories.Include(c => c.Tasks).ToList();
            List<Status> statuses = db.Statuses.ToList();

            // Получение задач из базы данных (например, из таблицы Tasks)
            List<Task> tasks = db.Tasks.ToList();

            // Передача данных в представление через ViewBag
            ViewBag.Statuses = statuses;
            ViewBag.Tasks = tasks;
            ViewBag.Сategories = categories;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}