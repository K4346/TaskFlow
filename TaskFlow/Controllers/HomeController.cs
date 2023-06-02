using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using TaskFlow.Models;

namespace TaskFlow.Controllers
{
    public class HomeController : Controller
    {


        private readonly ILogger<HomeController> _logger;
        private readonly IDataProtector _protector;

        private readonly TaskFlowContext db;

        public HomeController(ILogger<HomeController> logger, IDataProtectionProvider provider, TaskFlowContext _db)
        {
            _logger = logger;
            _protector = provider.CreateProtector("TaskFlow.Auth.v1");
            db = _db;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }
            Console.WriteLine("id"+int.Parse(User.FindFirst("UserId").Value));

            var users = db.Users.ToList();
            
            var tasks = db.Tasks.ToList().OrderByDescending(t => t.Priority).ToList();
            foreach (var task in tasks)
            {
                Console.WriteLine(task.AssignedtoNavigation.Username);
            }
            List<Category> categories = db.Categories.Include(c => c.Tasks).ToList();
            List<Status> statuses = db.Statuses.ToList();
            List<Priority> priorities = db.Priorities.ToList();
            ViewBag.Statuses = statuses;
            ViewBag.Tasks = tasks;
            ViewBag.Сategories = categories;

            return View();
        }

        public IActionResult TaskDetails(int taskId)
        {

            Task task = db.Tasks.FirstOrDefault(t => t.Taskid == taskId);
            if (task == null)
            {
                return NotFound();
            }

            var users = db.Users.ToList();
            users.ForEach(u =>
            {
                u.Username=Decrypt(u.Username);
            });

            User user = users.FirstOrDefault(u => u.Userid == task.Assignedto);
            ViewBag.User = user;
            return View(task);
        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Analyst")]
        public IActionResult TasksManager()
        {
 
            var tasks = db.Tasks.ToList().OrderByDescending(t => t.Priority).ToList();
            List<Status> statuses = db.Statuses.ToList();
            var categories = db.Categories.ToList();
            var priorities = db.Priorities.ToList();
            var users = db.Users.ToList();
            users.ForEach(u =>
            {
                u.Username = Decrypt(u.Username);
            });
            ViewBag.Statuses = statuses;
            ViewBag.Сategories = categories;
            ViewBag.Priorities = priorities;
            ViewBag.Users = users;
            return View(tasks);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Analyst")]
        public IActionResult SaveTask(int taskId, string title, string description, int statusId, int priorityId, int UserId, int CategoryId)
        {

            Console.WriteLine("SaveTask");
            var task = db.Tasks.Find(taskId);
            if (task != null && title!= null && description != null && statusId != null && priorityId != null && UserId != null && CategoryId != null)
            {
                Console.WriteLine("GOOD");
                task.Title = title;
                task.Description = description;
                task.Statusid = statusId;
                task.Priority = priorityId;
                task.Assignedto = UserId;
                task.Categoryid = CategoryId;
                db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Analyst")]
        public IActionResult CreateTask(int taskId, string title, string description, int statusId, int priorityId, int UserId, int CategoryId)
        {
            Console.WriteLine("CreateTask");
            if (title != null && title != "" && description != null && statusId != null && priorityId != null && UserId != null && CategoryId != null)
            {
                Task newTask = new Task();
                Console.WriteLine("GOOD");
                newTask.Title = title;
                newTask.Description = description;
                newTask.Statusid = statusId;
                newTask.Priority = priorityId;
                newTask.Assignedto = UserId;
                newTask.Categoryid = CategoryId;

                db.Tasks.Add(newTask);
                db.SaveChanges();
                return Ok();
            }

       

            return NotFound();
        }  
        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Analyst")]
        public IActionResult DeleteTask(int taskId)
        {
            Console.WriteLine("DeleteTask");
            var task = db.Tasks.Find(taskId);
            if (task != null)
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult ChangeTaskStatus(int taskId, int newStatus)
        {

            Task task = db.Tasks.FirstOrDefault(t => t.Taskid == taskId);

            Status status = db.Statuses.FirstOrDefault(t => t.Statusid == newStatus);



            if (task == null || status==null || task.Assignedto!= int.Parse(User.FindFirst("UserId").Value))
            {
                return NotFound();
            }

            
            task.Statusid = newStatus;
            db.SaveChanges();

        
            return RedirectToAction("Index");
        }

        public string Decrypt(string ciphertext)
        {
            return _protector.Unprotect(ciphertext);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}