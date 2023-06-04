using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;

namespace TaskFlow.Controllers
{
    public class AdminController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IDataProtector _protector;
        private readonly TaskFlowContext db;

       
        public AdminController(ILogger<HomeController> logger, IDataProtectionProvider provider, TaskFlowContext _db)
        {
            _logger = logger;
            _protector = provider.CreateProtector("TaskFlow.Auth.v1");
            db = _db;
        }


        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult AdminPanel()
        {
            var users = db.Users.ToList();
            var roles = db.Roles.ToList();
            users.ForEach(u =>
            {
                u.Username = Decrypt(u.Username);
            });

            ViewBag.Roles = roles.Where(r => r.Rolename != "Admin");
            return View(users.Where(u => u.Role.Rolename != "Admin"));
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> AddUser(string username, string password, string password2, string email, int role)
        {
            Console.WriteLine("AddUser");
            if (username == null || username == "" || password == null || password == "" || password!= password2)
            {
                return BadRequest();
            }
            string protectedLogin = _protector.Protect(username);
            Console.WriteLine($"login - Protect returned: {protectedLogin}");
            string protectedPassword = _protector.Protect(password);
            Console.WriteLine($"password - Protect returned: {protectedPassword}");
            var newUser = new User();
            newUser.Username = protectedLogin;
            newUser.Password = protectedPassword;
            newUser.Email = email;
            newUser.Roleid = role;
            db.Users.Add(newUser);
            db.SaveChanges();
            return RedirectToAction("AdminPanel","Admin");
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {

            var user = db.Users.Find(userId);
            Console.WriteLine("DeleteUser");
            Console.WriteLine(userId);
            if (user == null) {
                return BadRequest();
            }
            foreach (Task task in db.Tasks) {
            if (task.Assignedto == userId)
                {
                    db.Tasks.Remove(task);
                }
            }
            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("AdminPanel", "Admin");
        }

        public string Encrypt(string plaintext)
        {
            return _protector.Protect(plaintext);
        }

        public string Decrypt(string ciphertext)
        {
            return _protector.Unprotect(ciphertext);
        }
    }
}
