using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskFlow.Controllers;
using TaskFlow;
using Microsoft.AspNetCore.DataProtection;

namespace UniversitiesMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataProtector _protector;
        private readonly TaskFlowContext db;


        public AccountController(ILogger<HomeController> logger, IDataProtectionProvider provider, TaskFlowContext _db)
        {
            _logger = logger;
            _protector = provider.CreateProtector("TaskFlow.Auth.v1");
            db = _db;
        }

   

        [HttpPost("/token")]
        public IActionResult Token(string username, string password)
        {
            //if (username == null || password == null || !IsBase64String(username) || !IsBase64String(password)) {
            //    //todo
            //    Console.WriteLine(username);
            //    Console.WriteLine(IsBase64String(username));
            //    return Unauthorized(new { errorText = "Inputs have invalid characters" });

            //}
            // protect the payload
            string protectedLogin = _protector.Protect(username);
            Console.WriteLine($"login - Protect returned: {protectedLogin}");
            string protectedPassword = _protector.Protect(password);
            Console.WriteLine($"password - Protect returned: {protectedPassword}");

            // unprotect the payload
            string unprotectedLogin = _protector.Unprotect(protectedLogin);
            Console.WriteLine($"login - Unprotect returned: {unprotectedLogin}");   
            string unprotectedPassword = _protector.Unprotect(protectedPassword);
            Console.WriteLine($"password - Unprotect returned: {unprotectedPassword}");

            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                return Unauthorized(new { errorText = "Invalid username or password" });
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            HttpContext.SignInAsync(claimsPrincipal).Wait();

            return Ok(new { redirectTo = Url.Action("Index", "Home") });
        }

        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            Console.WriteLine("logout");
            HttpContext.SignOutAsync().Wait();
            Console.WriteLine(User.Identity.IsAuthenticated);
            Console.WriteLine(User.Identity.Name);
            return Ok();
        }


        private ClaimsIdentity GetIdentity(string username, string password)
        {
            var users = db.Users.ToList();
            var roles = db.Roles.ToList();
        
            var person = users.FirstOrDefault(x => Decrypt(x.Username) == username && Decrypt(x.Password) == password);
            if (person != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, Decrypt(person.Username)),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role.Rolename)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }

            // если пользователя не найдено
            return null;
        }

        public string Encrypt(string plaintext)
        {
            return _protector.Protect(plaintext);
        }

        public string Decrypt(string ciphertext)
        {
            return _protector.Unprotect(ciphertext);
        }

        public bool IsBase64String(string input)
        {
            try
            {
    
                byte[] buffer = Convert.FromBase64String(input);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
