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

namespace LoginReg.Controllers
{
    public class DashboardController : Controller
    {
        private LoginRegContext dbContext;

        public DashboardController(LoginRegContext context)
        {
            dbContext = context;
        }

        [Route("dashboard")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            List<Event> AllEvents = dbContext.Events
                .Include(w => w.Guests)
                .OrderBy(w => w.EventDate).ThenBy(w => w.EventTime)
                .ToList();
            ViewBag.UserId = userId;

            return View("Dashboard", AllEvents);
        }

        [Route("add/event")]
        [HttpGet]
        public IActionResult AddEvent()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            return View("AddEvent");
        }

        [Route("create/event")]
        [HttpPost]
        public IActionResult CreateEvent(Event newEvent)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            if (ModelState.IsValid)
            {
                var oneUser = dbContext.Users.FirstOrDefault(w => w.UserId == userId);
                newEvent.CreatorName = oneUser.FirstName;
                newEvent.UserId = (int)userId;
                dbContext.Add(newEvent);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }

            return View("AddEvent");
        }

        [Route("join/event/{eventId}")]
        [HttpGet]
        public IActionResult JoinEvent(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            Event oneEvent = dbContext.Events
                .Include(w => w.Guests)
                .ThenInclude(a => a.User)
                .FirstOrDefault(w => w.EventId == eventId);

            User oneUser = dbContext.Users
                .Include(u => u.UserEvents)
                .ThenInclude(a => a.Event)
                .FirstOrDefault(u => u.UserId == userId);

   
            Association newAssociation = new Association()
            {
                UserId = (int)userId,
                EventId = eventId
            };

            dbContext.Associations.Add(newAssociation);
            oneEvent.Guests.Add(newAssociation);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [Route("leave/event/{eventId}")]
        [HttpGet]
        public IActionResult LeaveEvent(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            var oneAssociation = dbContext.Associations
                .FirstOrDefault(a => a.EventId == eventId && a.UserId == userId);

            dbContext.Associations.Remove(oneAssociation);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        [Route("view/{eventId}")]
        [HttpGet]
        public IActionResult ViewEvent(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ValidateUserLoggedIn(userId);

            Event oneEvent = dbContext.Events
                .Include(w => w.Guests)
                .ThenInclude(a => a.User)
                .FirstOrDefault(w => w.EventId == eventId);
            ViewBag.UserId = userId;

            return View("ViewEvent", oneEvent);
        }

        [Route("/delete/{eventId}")]
        [HttpGet]
        public IActionResult DeleteEvent(int eventId)
        {
            Event oneEvent = dbContext.Events.FirstOrDefault(w => w.EventId == eventId);
            dbContext.Remove(oneEvent);
            dbContext.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        private IActionResult ValidateUserLoggedIn(int? userId)
        {
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return NoContent();
        }
    }
}
