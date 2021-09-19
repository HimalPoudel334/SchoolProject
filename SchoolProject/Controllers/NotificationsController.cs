using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SchoolProject.Data;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> UserNotificationsCountAsync() //the Async suffix will be ignored and hence ignore it while requesting or creatig view
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var notifications = await _context.Notifications.AsQueryable()
                .Where(n => n.User.Id == user.Id)
                .GroupBy(g => g.NotificationType)
                .Select(s => new { TypeOfNoti = s.Key, Count = s.Count() })
                .ToListAsync();
            return Json(notifications);
        }

        public async Task<JsonResult> UserNotificationsAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var notifications = await _context.Notifications.AsQueryable()
                .Where(n => n.User.Id == user.Id)
                .Select(s => new { UserId = s.User.Id, MedicineId = s.Medicine.Id, s.Text, s.Seen, TypeOfNoti = s.NotificationType })
                .ToListAsync();
            return Json(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int type, string username)
        {
            var notifications = await _context.Notifications.Include(n => n.User).AsQueryable()
                .Where(n => n.User.UserName == username && n.NotificationType == (Models.Type)type)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            //return Ok();
            return Json(notifications);
        }

        //, ActionName("Delete")
    }
}
