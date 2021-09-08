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

        // GET: Donations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Donations.Include(d => d.Medicine).Include(d => d.Donor).Include(d => d.ReceiverNgo).ToListAsync());
        }

        public async Task<IActionResult> DonatedMedicines()
        {
            var donates = await _context.Donations.Include(d => d.Medicine).Where(d => d.Completed).ToListAsync();
            return View(donates);
        }

        public async Task<IActionResult> MyDonations()
        {
            var user = await _userManager.GetUserAsync(this.User);
            try {
                var donates = await _context.Donations.Include(d => d.Medicine).Where(d => d.Donor.Id == user.Id).ToListAsync();
                return View(donates);
            }
            catch (Exception)
            {
                return View();
            }
        }

        // GET: Donations/Invoice/5
        public async Task<IActionResult> Invoice(int? id)
        {
            /*int? donationId = Convert.ToInt32(HttpContext.Session.GetString("donationId"));
            if (id == null || donationId == null || donationId != id)
            {
                HttpContext.Session.Remove("donationId");
                return NotFound();
            }*/
            if (id == null) return NotFound();

            var donation = await _context.Donations.Include(d => d.Donor).Include(d => d.Medicine).Include(d => d.ReceiverNgo)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donation == null)
            {
                HttpContext.Session.Remove("donationId");
                return NotFound();
            }

            HttpContext.Session.Remove("donationId");
            return View(donation);
        }

        // GET: Donations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
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
                        ngos = await _context.Ngos.Where(n => n.Id != user.Id).ToListAsync();
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
                    donation.QuantityRemaining = donation.Quantity;

                    _context.Add(donation);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("donationId", donation.Id.ToString());
                    return RedirectToAction(nameof(Invoice), donation.Id);

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

            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return NotFound();
            }
            return View(donation);
        }

        // POST: Donations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Quantity,DonationTime,Completed")] Donation donation)
        {
            if (id != donation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonationExists(donation.Id))
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
            return RedirectToAction(nameof(Index));
        }

        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.Id == id);
        }
    }
}
