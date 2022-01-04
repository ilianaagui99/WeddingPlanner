using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Models;
//extra
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using  Microsoft.AspNetCore.Server.Kestrel.Core;

//extra probably for sessions
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
       private MyContext _context;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            _context = context;
        }
        //index page has both registration and login
        [HttpGet("")] //basically like combining get actions for 
        public IActionResult Index()
        {
            return View();
        }
        //Get Dashboard
        [HttpGet("Dashboard")]
         public IActionResult Dashboard()
            {
                if (HttpContext.Session.GetInt32("userId") == null)
                {
                    return RedirectToAction("Index");
                }
                List<Wedding> EveryWedding = _context.Weddings
                .Include(w => w.UsersWhoRSVP)
                .ThenInclude(a => a.User)
                .ToList();

                ViewBag.AllWeddings = EveryWedding;
                ViewBag.UserId = (int)HttpContext.Session.GetInt32("userId");
                return View("Dashboard");
             

            }
        
        //Get single wedding info
        [HttpGet("SingleWedding/{weddId}")]
        public IActionResult SingleWedding(int weddId)
            {
                // get the selected wedding info
                ViewBag.SpecificWedding = _context.Weddings
                    .FirstOrDefault(w => w.WeddingId == weddId);

                // Get all guests from wedding
                var weddingGuests = _context.Weddings
                .Include(w => w.UsersWhoRSVP)
                .ThenInclude(u => u.User)
                .FirstOrDefault(w => w.WeddingId == weddId);
            
            ViewBag.AllGuests = weddingGuests.UsersWhoRSVP;
            return View("SingleWedding");
            }
        //Show add wedding
        [HttpGet("AddWedding")]
        public IActionResult AddWedding()
            {
                if (HttpContext.Session.GetInt32("userId") == null)
                {
                    return RedirectToAction("Index");
                }
                return View("AddWedding");
            }
        
        //Post to login
        [HttpPost("LoginPost")] //try changing to /login
        public IActionResult LoginPost(Login userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                //ViewBag.ThisUser = userInDb; //CHECKKK
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<Login>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Password", "Invalid Password");
                    return View("Login");
                }
                // assign user ID to sessions
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            // go back to login if fails
            return View("Index");
        }

        //Post to registration
        [HttpPost("Registration")]
        public IActionResult Registration(User user)
        {
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // if everything is okay save the user to the DB
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                _context.Add(user);
                _context.SaveChanges();
                User userInDb = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }
            // other code
            return View("Index");
        }

        //Post to AddWedding
        [HttpPost("AddWeddingPost")]
        public IActionResult AddWeddingPost(Wedding newWedding)
            {   
               if (ModelState.IsValid)
                {
                    newWedding.UserId = (int)HttpContext.Session.GetInt32("userId");
                    _context.Add(newWedding);
                    _context.SaveChanges();
                    Wedding thisWedding = _context.Weddings.OrderByDescending(w => w.CreatedAt).FirstOrDefault();
                    return Redirect("/Wedding/"+thisWedding.WeddingId);

                }

                return View("AddWedding", newWedding);
            }
            

        //RVSP
        [HttpGet("rsvp/{weddId}")]
        public IActionResult RSVP(int weddId)
        {
            Association attendance = new Association();
            attendance.UserId = (int)HttpContext.Session.GetInt32("userId");
            attendance.WeddingId = weddId;
            _context.Associations.Add(attendance);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        //Un-RVSP
        [HttpGet("unrsvp/{attId}")]
        public IActionResult UnRSVP(int attId)
        {
            Association attendance = _context.Associations.FirstOrDefault(a => a.AssociationId == attId);
            _context.Associations.Remove(attendance);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        
        //Delete
        [HttpGet("delete/{weddingId}")]
        public IActionResult DeleteWedding(int weddingId)
            {
                // find the wedding want to delete 
                Wedding RetrievedWedding = _context.Weddings
                    .FirstOrDefault(wed => wed.WeddingId == weddingId);
                _context.Weddings.Remove(RetrievedWedding);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");

            }
        
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
}
}

