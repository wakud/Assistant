using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Assistant_TEP.Models;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Assistant_TEP.MyClasses;
using Microsoft.AspNetCore.Authorization;

namespace Assistant_TEP.Controllers
{
    public class AccountController : Controller
    {
        private readonly MainContext db;
        public AccountController(MainContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(ModelState.IsValid)
            {
                string encryptedData = Utils.Encrypt(model.Password);
                string decryptedData = Utils.Decrypt(encryptedData);
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == encryptedData);
                //Console.WriteLine("Zakodovanyj parol - " + encryptedData);
                //Console.WriteLine("Rozkodovanyj parol - " + decryptedData);
                //User user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);

                if (user != null)
                {
                  
                    await Authenticate(user); // авторизація
                    return RedirectToAction("Index", "Home");
                } 
                else
                {
                    ModelState.AddModelError("", "Не вірний логін і(або) пароль");
                    ViewBag.Error = "BedLogin";
                }
            }
            return View(model);
        }

        private async Task Authenticate(User user)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim("AdminStatus", user.IsAdmin)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            //  вихід з програми
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Admin/Create
        [Authorize(Policy = "OnlyForAdministrator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
