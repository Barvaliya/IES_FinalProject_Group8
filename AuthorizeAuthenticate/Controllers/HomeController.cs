using AuthorizeAuthenticate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Owin.Security.Provider;
using MySqlConnector;
using System.Diagnostics;
using YourNamespace;
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



    public IActionResult SubmitForm(string username, string password)
        {

            bool isValid = IsUserValidAsync(username, password);

            if (isValid)
            {
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("Welcome", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
  
        public IActionResult Welcome()
        {
            string username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        private bool IsUserValidAsync(string username, string password)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            var query = $"SELECT COUNT(*) FROM users WHERE user_name = '{username}' AND password = '{password}'";
            Console.WriteLine(query);
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