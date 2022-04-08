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
        /// <summary>
        /// Вхід у програму
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if(ModelState.IsValid)
            {
                string encryptedData = Utils.Encrypt(model.Password);
                string decryptedData = Utils.Decrypt(encryptedData);
                User user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == encryptedData);

                if (user != null)
                {
                  
                    await Authenticate(user); 
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
        /// <summary>
        /// Підтвердження входу
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim("AdminStatus", user.IsAdmin)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        /// <summary>
        /// вихід з програми
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// тільки для адміністратора
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "OnlyForAdministrator")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
