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
            try
            {
                var Message = TempData["Message"] as string;
                ViewBag.Message = Message;
            }
            catch(Exception e) 
            {
                Console.WriteLine("message not passed.");
            }           

            var data = dbConnector.FetchDataUsingReader(contactQuery);
            return View(data);
        }

        [HttpPost]
        public ActionResult ContactId(string contactId, string action)
        {
            // Use the contactId and action variables in your controller logic
            Console.WriteLine("ContactId received in controller: " + contactId);
            Console.WriteLine("Action received in controller: " + action);

            string query = "";

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            if (action == "delete") {
                query = $"DELETE FROM contacts WHERE ContactId = {contactId};";
          
                if (dbConnector.GetCount(query)> 0) {
                    TempData["Message"] = "Operation was successful!";
                    return RedirectToAction("Welcome", "Home");
                }
                else
                {

                }                
            }
            else if (action == "edit")
            {

            }
            else if (action == "details")
            {
                query = $"select * from contacts where ContactId = {contactId};";
                var data = dbConnector.FetchDataUsingReader(query);
                TempData["contactId"] = contactId;
            }
            return RedirectToAction("Index");
        }

        //public IActionResult Detail() {
        //Console.WriteLine("At Details");
        //var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        //var dbConnector = new DatabaseConnector(config);
        //string id = TempData["contactId"].ToString();
        //string query = $"select * from contacts where ContactId = {id};";
        //var data = dbConnector.FetchDataUsingReader(query);
        //return View();
        //}

        [HttpPost]
        public ActionResult HandleApproval(int contactId, string action)
        {
            Console.WriteLine("HandleApproval ma pugyo");
           var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);
            // Handle the received data here
            if (action == "approve")
            {
                Console.WriteLine("Approved ma pugyo");
                if (dbConnector.GetCount($"UPDATE contacts SET status = 1 WHERE contactid = {contactId};") > 0)
                {
                    return RedirectToAction("home", "welcome");
                }
                Console.WriteLine("Approved ko query run bhayena");
                return Json(new { success = false, message = $"Contact ID {contactId} has not been approved" });
            }
            else if (action == "reject")
            {
                Console.WriteLine("Rejected ma pugyo");
                if (dbConnector.GetCount($"UPDATE contacts SET status = 0 WHERE contactid = {contactId};") > 0)
                {
                    return RedirectToAction("home", "welcome");
                }
                Console.WriteLine("Rejected ko query run bhayena");
                return Json(new { success = false, message = $"Contact ID {contactId} has not been rejected" });
            }
            else
            {
                Console.WriteLine(action);
                Console.WriteLine(contactId);
                return Json(new { success = false, message = "Invalid action" });
            }
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