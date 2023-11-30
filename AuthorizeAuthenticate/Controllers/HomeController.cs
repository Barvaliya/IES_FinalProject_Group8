using AuthorizeAuthenticate.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Owin.Security.Provider;
using MySqlConnector;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Security.Claims;
using YourNamespace;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace AuthorizeAuthenticate.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }



    public async Task<IActionResult> SubmitFormAsync(string username, string password)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            bool isValid = IsUserValidAsync(username, password);

            if (isValid)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                string query = $"select user_role from users where user_name ='{username}'";
                string user_type = dbConnector.ExecuteDirectQuery(query);
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("user_type", user_type);

                return RedirectToAction("Welcome", "Home");         
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        //[Authorize]
        public IActionResult Welcome()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            string username = HttpContext.Session.GetString("Username");
            string user_type = HttpContext.Session.GetString("user_type");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.user_type = user_type;
            ViewBag.user_name = username;

            string contactQuery = "";
           
            if (user_type == "a")
            {
                contactQuery = "SELECT * FROM contacts";
            }
            else if (user_type == "r")
            {
                // Registered users can view all the approved data and can edit/ delete their own data.
                contactQuery = $"SELECT * FROM contacts where status='1' && creator='{username}'";
            }
            else if (user_type == "m")
            {
                //Managers can approve or reject contact data.Only approved contacts are visible to users.
                contactQuery = "SELECT * FROM contacts";
            }

            var data = dbConnector.FetchDataUsingReader(contactQuery);               

            return View(data);
        }

        private bool IsUserValidAsync(string username, string password)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            var query = $"SELECT COUNT(*) FROM users WHERE user_name = '{username}' AND password = '{password}'";
            //Console.WriteLine(query);
            int count = dbConnector.GetCount(query);

            if (count > 0)
            {
                Console.WriteLine("User authenticated!");
                return true;
            }
            else
            {
                // Validation failed
                Console.WriteLine("Invalid username or password.");
                return false;
            }        
        }
    }
}