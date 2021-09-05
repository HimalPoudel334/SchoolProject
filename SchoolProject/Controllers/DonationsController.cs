using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Create(int id)
        {
            Medicine med = await _context.Medicines.FindAsync(id);

            if (med != null)
            {
                //var ngos = await _userManager.Users.OfType<Ngo>().ToListAsync();
                var ngos = await _context.Ngos.ToListAsync();
                ViewBag.NgoList = new SelectList(ngos, "Id", "Name");

                var donation = new Donation()
                {
                    Medicine = med
                };
                return View(donation);
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
                //donation.Id = 0;
                donation.Medicine = await _context.Medicines.FindAsync(donation.Medicine.Id);
                donation.DonationTime = DateTime.UtcNow;
                var loggedInUser = await _userManager.GetUserAsync(this.User);
                donation.Donor = await _context.Donors.FindAsync(loggedInUser.Id);
                donation.ReceiverNgo = await _context.Ngos.FindAsync(donation.ReceiverNgo.Id);

                _context.Add(donation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

                //return Json(donation);  
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
