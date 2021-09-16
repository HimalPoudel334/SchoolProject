using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SchoolProject.Data;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; 
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> AnalyticsAsync()
        {
            var allDonations = _context.Donations.AsQueryable();
            var allRequests = _context.Requests.AsQueryable();

            //most medicine donations
            var mostDonations = await allDonations
                .GroupBy(d => d.Medicine.Name)
                .Select(g => new { name = g.Key, count = g.Count() }).ToListAsync();
            ViewBag.MostDonations = JsonConvert.SerializeObject(mostDonations);

            //most medicine requests
            var mostRequests = await allRequests
                .GroupBy(d => d.Medicine.Name)
                .Select(g => new { name = g.Key, count = g.Count() }).ToListAsync();
            ViewBag.MostRequests = JsonConvert.SerializeObject(mostRequests);

            var today = DateTime.Today;
            var todaysDonations = await allDonations
                .Include(d => d.Donor)
                .Include(d => d.Medicine)                    
                .Include(d => d.ReceiverNgo)                    
                .Where(d => d.DonationTime.Day == today.Day)
                .ToListAsync();
            ViewBag.TodaysDonations = todaysDonations;

            var todaysRequests = await allRequests
                .Include(d => d.Requestor)
                .Include(d => d.Medicine)
                .Include(d => d.RequestingNgo)
                .Where(d => d.RequestDate.Day == today.Day)
                .ToListAsync();
            ViewBag.TodaysRequests = todaysRequests;

            //this month transactions
            var monthsDonations = await allDonations
                .Where(d => d.DonationTime.Year == today.Year && d.DonationTime.Month == today.Month)
                .CountAsync();

            var monthsRequests = await allRequests
                .Where(r => r.RequestDate.Year == today.Year && r.RequestDate.Month == today.Month)
                .CountAsync();

            var monthTrans = new
            {
                Donations = monthsDonations,
                Requests = monthsRequests
            };
            ViewBag.MonthsTransactions = JsonConvert.SerializeObject(monthTrans);
            
            //total requests over the year
            var requests = await allRequests
                .GroupBy(r => r.RequestDate.Month)
                .Select(g => new { month = g.Key, count = g.Count() }).ToListAsync();

            //total requests over the year
            var donations = await allDonations
                .GroupBy(d => d.DonationTime.Month)
                .Select(g => new { month = g.Key, count = g.Count() }).ToListAsync();

            for (int i = 0; i <= 12; i++)
            {
                try
                {
                    if(requests[i].month != i && requests[i].count != 0)
                    {
                        requests.Insert(i, new { month = i, count = 0 });
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    requests.Insert(i, new { month = i, count = 0 });
                }

                try
                {
                    if(donations[i].month != i && donations[i].count != 0)
                    {
                        donations.Insert(i, new { month = i, count = 0 });
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    donations.Insert(i, new { month = i, count = 0 });
                }

            }

            ViewBag.Requests = JsonConvert.SerializeObject(requests);
            ViewBag.Donations = JsonConvert.SerializeObject(donations);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
