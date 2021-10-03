using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SchoolProject.Data;
using SchoolProject.Models;

namespace SchoolProject.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DonationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        // GET: Donations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Donations.AsQueryable().Include(d => d.Medicine).Include(d => d.Donor).Include(d => d.ReceiverNgo).ToListAsync());
        }

        public async Task<IActionResult> DonatedMedicines()
        {
            var donates = await _context.Donations.AsQueryable().Include(d => d.Medicine).Where(d => d.Completed).ToListAsync();
            return View(donates);
        }

        [HttpGet]
        public async Task<IActionResult> MyDonations()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<Donation> donations = null;
            try {
                if (await _userManager.IsInRoleAsync(user, "Ngo"))
                {
                    donations = await _context.Donations.AsQueryable().Include(d => d.Medicine).Include(d => d.Donor).Where(d => d.ReceiverNgo.Id == user.Id).ToListAsync();
                }
                else
                { 
                    donations = await _context.Donations.AsQueryable().Include(d => d.Medicine).Include(d => d.ReceiverNgo).Where(d => d.Donor.Id == user.Id).ToListAsync();
                }
                return View(donations);
            }
            catch (Exception)
            {
                return View();
            }
        }

        // GET: Donations/Invoice/5
        public async Task<IActionResult> Invoice()
        {
            //uncomment this and add int? id paramter to the function if want to generate invoice for individual donation
            /*int? donationId = Convert.ToInt32(HttpContext.Session.GetString("donationId"));
            if (id == null || donationId == null || donationId != id)
            {
                HttpContext.Session.Remove("donationId");
                return NotFound();
            }*/

            var user = await _userManager.GetUserAsync(this.User);
            var donation = await _context.Donations.AsQueryable().Include(d => d.Donor).Include(d => d.Medicine).Include(d => d.ReceiverNgo)
                .Where(m => m.Donor.Id == user.Id && !m.Completed).ToListAsync();
            if (donation == null)
            {
                HttpContext.Session.Remove("donationId");
                return NotFound();
            }

            HttpContext.Session.Remove("donationId");
            ViewBag.Donor = user;
            
            return View(donation);
        }

        // GET: Donations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            var donation = await _context.Donations.AsQueryable().Include(d => d.Medicine).Include(d => d.Donor).Include(d => d.ReceiverNgo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // GET: Donations/Create
        public async Task<IActionResult> Create(Medicine medicine)
        {
            try
            {
                var medFromSession = JsonConvert.DeserializeObject<Medicine>(HttpContext.Session.GetString("medicine"));
                if (medicine != null && medFromSession != null && medFromSession.Equals(medicine))
                {
                    List<Ngo> ngos = null;

                    if (this.User.IsInRole("Ngo"))
                    {
                        var user = await _userManager.GetUserAsync(this.User);
                        ngos = await _context.Ngos.AsQueryable().Where(n => n.Id != user.Id).ToListAsync();
                    }
                    else
                    {
                        ngos = await _context.Ngos.ToListAsync();
                    }
                    
                    ViewBag.NgoList = new SelectList(ngos, "Id", "Name");

                    var donation = new Donation()
                    {
                        Medicine = medicine
                    };
                    return View(donation);
                }
            }catch(Exception)
            {
                return NotFound();
            }
            return NotFound();
        }

        // POST: Donations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Medicine,ReceiverNgo,Quantity")] Donation donation)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medFromSession = JsonConvert.DeserializeObject<Medicine>(HttpContext.Session.GetString("medicine"));
                    HttpContext.Session.Remove("medicine");

                    donation.Medicine = medFromSession;
                    var loggedInUser = await _userManager.GetUserAsync(HttpContext.User);
                    donation.Donor = await _context.Donors.FindAsync(loggedInUser.Id);
                    donation.ReceiverNgo = await _context.Ngos.FindAsync(donation.ReceiverNgo.Id);
                    _context.Add(donation);
                    donation.QuantityRemaining = donation.Quantity;
                    //for notification
                    Notification notification = new()
                    {
                        NotificationType = Models.Type.Donation,
                        Text = $"{donation.Medicine.Name} medicine donated by {donation.Donor.FirstName} {donation.Donor.LastName}",
                        User = donation.ReceiverNgo,
                        Medicine = medFromSession
                    };
                    _context.Add(notification);

                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("donationId", donation.Id.ToString());
                    TempData["flashMessage"] = "Donation Successful";
                    return RedirectToAction(nameof(MyDonations));

                }
                catch (Exception)
                {
                    return View(donation);
                }

            }
          
            return View(donation);
        }

        // GET: Donations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations.Include(d => d.Medicine).Include(d => d.Donor).Include(d => d.ReceiverNgo)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (donation == null)
            {
                return NotFound();
            }
            return View(donation);
        }

        // POST: Donations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Medicine,ReceiverNgo,Donor,Quantity,DonationTime,Completed")] Donation donation)
        {
            if (id != donation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //send notification to donor if completed is updated to true
                    if (donation.Completed)
                    {
                        //too much round trip to database. Worst codes ever
                        donation.ReceiverNgo = await _context.Ngos.FindAsync(donation.ReceiverNgo.Id);
                        donation.Donor = await _context.Donors.FindAsync(donation.Donor.Id);
                        donation.Medicine = await _context.Medicines.FindAsync(donation.Medicine.Id);
                        Notification notification = new()
                        {
                            Medicine = donation.Medicine,
                            Text = $"Your donation of {donation.Medicine.Name} is received by {donation.ReceiverNgo.Name}",
                            User = donation.Donor,
                            NotificationType = Models.Type.Donation
                        };
                        _context.Add(notification);
                    }
                    _context.Update(donation);
                    await _context.SaveChangesAsync();
                    TempData["flashMessage"] = "Update successful";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DonationExists(donation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(donation);
        }

        // GET: Donations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // POST: Donations/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            _context.Donations.Remove(donation);
            await _context.SaveChangesAsync();
            TempData["flashMessage"] = "Deletion successfull";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> DonationExists(int id)
        {
            return await _context.Donations.AnyAsync(e => e.Id == id);
        }

        //jabarjasti method
        public async Task<IActionResult> GetDetailsFromMedicineId(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Medicine.Id == id);
            if(donation != null)
                return RedirectToAction(nameof(Details), new { donation.Id });
            return NotFound();
        }

        //very bad but don't touch it
        public async Task<IActionResult> Search(string q, string page)
        {
            var pages = new List<string>() //ya unnecessary memory consumption
            {
                "Index", "MyDonations", "DonatedMedicines"
            };
            if (string.IsNullOrEmpty(page) || !pages.Contains(page))
            {
                return NotFound();
            }
            if(string.IsNullOrEmpty(q))
            {
                return RedirectToAction(page);
            }

            q = q.ToLower();
            var user = await _userManager.GetUserAsync(this.User);
            IQueryable<Donation> allDonations = null;
            IEnumerable<Donation> donations = null;
            
            //yea i know its cheating, but upaya tha xaina
            if (page.Equals("MyDonations"))
            {
                if(this.User.IsInRole("Ngo"))
                    allDonations = _context.Donations.Include(d => d.Medicine).Include(d => d.ReceiverNgo).Where(d => d.ReceiverNgo.Id == user.Id).AsQueryable();
                else
                    allDonations = _context.Donations.Include(d => d.Medicine).Include(d => d.ReceiverNgo).Where(d => d.Donor.Id == user.Id).AsQueryable();
            }
            else if (page.Equals("Index"))
            {
                allDonations = _context.Donations.Include(d => d.Medicine).Include(d => d.ReceiverNgo).Include(d => d.Donor).Include(d => d.ReceiverNgo).AsQueryable();
            }
            else if (page.Equals("DonatedMedicines"))
            {
                allDonations = _context.Donations.Include(d => d.Medicine).Include(d => d.ReceiverNgo).Where(d => d.Completed).AsQueryable();
            }
            try
            {
                donations = await allDonations
                    .Where(d => d.Medicine.Name.ToLower().Contains(q) ||
                        d.Medicine.GenericName.ToLower().Contains(q) ||
                        d.Medicine.Description.ToLower().Contains(q))
                        .ToListAsync();
                if (!donations.Any())
                {
                    TempData["flashMessage"] = "No such donations found";
                    return RedirectToAction(page);
                }
                return View(page, donations);
            }
            catch (Exception)
            {
                TempData["flashMessage"] = "No such donations found";
                return View(page, allDonations);
            }
        }
    }
}
