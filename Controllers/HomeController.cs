using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginReg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LoginReg.Controllers
{
    public class HomeController : Controller
    {
        private LoginRegContext _dbContext;
        private PasswordHasher<IUser> _passwordHasher;

        public HomeController(LoginRegContext context, PasswordHasher<IUser> passwordHasher)
        {
            _dbContext = context;
            _passwordHasher = passwordHasher;
        }

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }

        [Route("create")]
        [HttpPost]
        public IActionResult Register(User newUser)
        {
            if (ModelState.IsValid)
            {
                var userInDb = _dbContext.Users.FirstOrDefault(u => u.Email == newUser.Email);
                if (userInDb != null)
                {
                    ModelState.AddModelError("Email", "This email already taken");
                    return View("Index");
                }

                newUser.Password = _passwordHasher.HashPassword(newUser, newUser.Password);
                _dbContext.Add(newUser);
                _dbContext.SaveChanges();
                var userToLogIn = _dbContext.Users.FirstOrDefault(u => u.Email == newUser.Email);
                HttpContext.Session.SetInt32("UserId", userToLogIn.UserId);
                return RedirectToAction("Dashboard", "Dashboard");
            }
            else
            {
                return View("Index");
            }
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login(LoginUser userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }

                // verify provided password against hash stored in db
                var result = _passwordHasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                    return RedirectToAction("Dashboard", "Dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [Route("logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        [Route("ggggg")]
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            return Json(claims);
        }

        public async Task<IActionResult> LogoutGoogle()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
