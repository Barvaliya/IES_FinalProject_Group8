﻿using AuthorizeAuthenticate.Models;
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
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                //get the row with the certain contact detail and pass it to a method
                query = $"select * from contacts where ContactId={contactId}";
                var data = dbConnector.ExecuteDirectQuery(query);
                TempData["data"] = data;
                return RedirectToAction("Forms", "Home");


            }
            else if (action == "details")
            {
                query = $"select * from contacts where ContactId = {contactId};";
                var data = dbConnector.FetchDataUsingReader(query);
                TempData["contactId"] = contactId;
            }
            return RedirectToAction("Index");
        }

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

        public IActionResult Form() {
            string username = HttpContext.Session.GetString("Username");
            string user_type = HttpContext.Session.GetString("user_type");

            ViewBag.user_type = user_type;
            ViewBag.user_name = username;

            try {
                var myData = TempData["data"] as string;
                ViewBag.MyData = myData;
            }
            catch(Exception e) {
                Console.WriteLine("Data not found");
            }

            return View();
        }

        [HttpPost]
        public IActionResult HandleForm(Contact contact) {

            // Process the submitted contact data (contact.Name, contact.Address, etc.)
            // Save to the database, perform actions, etc.
            Console.WriteLine(contact.Name);

             string username = HttpContext.Session.GetString("Username");

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            string query = $"INSERT INTO contacts  (ContactId, Name, Address, City, State, Zip, Email, Status, creator) VALUES (NULL, '{contact.Name}', '{contact.Address}', '{contact.City}', '{contact.State}' ,'{contact.Zip}', '{contact.Email}', '2' , '{username}')";

            if(dbConnector.GetCount(query)>0){
                return RedirectToAction("Welcome", "Home");
            }
            ViewBag.message = "Invalid Input. Please Check."; 
            return RedirectToAction("Form", "Home");        
        }

        [HttpGet("form")]
        public IActionResult EditForm([FromBody] Dictionary<string, string> requestData)
        {
            /*
            //string param1 = HttpUtility.ParseQueryString(url.Query).Get("id");

            string[] parts = url.Split('=');

            // Retrieve the value after the equals sign
            string valueAfterEquals = parts[parts.Length - 1];

            Console.WriteLine(valueAfterEquals);*/

            //var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");

            //var url = location.AbsoluteUri.ToString();

            if (requestData != null && requestData.ContainsKey("contactId"))
            {
                string contactId = requestData["contactId"];
                // Perform actions with the received contactId, such as updating data, etc.
                // Example: UpdateContact(contactId);
                Console.WriteLine(contactId);
                return RedirectToAction("Form", "Home");

            }

            return RedirectToAction("Form", "Home");
        }

        public IActionResult User()
        {
            string username = HttpContext.Session.GetString("Username");
            string user_type = HttpContext.Session.GetString("user_type");

            ViewBag.user_type = user_type;
            ViewBag.user_name = username;

            var roles = new List<SelectListItem>
            {
                new SelectListItem { Text = "Admin", Value = "a" },
                new SelectListItem { Text = "Manager", Value = "m" },
                new SelectListItem { Text = "Registered User", Value = "r" }
            };

            // Pass the roles list to the view
            ViewBag.UserRoles = roles;
            return View();
        }

        [HttpPost]
        public IActionResult HandleUsers(Users user)
        {
            Console.WriteLine("Why not here?");
            Console.WriteLine(user.user_role);
            Console.WriteLine(user.user_name);

            string username = HttpContext.Session.GetString("Username");

            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            var dbConnector = new DatabaseConnector(config);

            string query = $"INSERT INTO users  (user_id, user_name, pasword, user_role) VALUES (NULL, '{user.user_name}', '{user.password}', '{user.user_role}')";

            if (dbConnector.GetCount(query) > 0)
            {
                return RedirectToAction("Welcome", "Home");
            }
            ViewBag.message = "Invalid Input. Please Check.";
            return RedirectToAction("User", "Home");
        }
    }
}