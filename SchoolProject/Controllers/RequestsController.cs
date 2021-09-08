using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models;

namespace SchoolProject.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private static Donation don;

        public RequestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Requests
        public async Task<IActionResult> Index()
        {
            return View(await _context.Requests.Include(d => d.Medicine).Include(d => d.Requestor).Include(d => d.RequestingNgo).ToListAsync());
        }

        // GET: Requests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Requests/Create
        public async Task<IActionResult> Create(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            try
            {
                don = await _context.Donations.Include(d => d.ReceiverNgo).Include(d => d.Medicine).FirstAsync(d => d.Id == id);
                var request = new Request()
                {
                    Medicine = don.Medicine,
                    RequestingNgo = don.ReceiverNgo
                };

                return View(request);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: Requests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Medicine,RequestingNgo,Quantity")] Request request)
        {
            if (ModelState.IsValid)
            {
                var donation = await _context.Donations.FindAsync(don.Id);
                if (request.Quantity > donation.QuantityRemaining || request.Quantity <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Only " + don.QuantityRemaining + " Medicines left in stock");
                    ModelState.AddModelError(string.Empty, "Quantity must be greater than zero");
                    request.Medicine = don.Medicine;
                    request.RequestingNgo = don.ReceiverNgo;
                    return View(request);
                }

                
                donation.QuantityRemaining -= request.Quantity;
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var requestor = await _context.Donors.FindAsync(user.Id);
                var donorNgo = await _context.Ngos.FindAsync(request.RequestingNgo.Id);
                var medicine = await _context.Medicines.FindAsync(request.Medicine.Id);

                request.RequestDate = DateTime.Now;
                request.Medicine = medicine;
                request.Requestor = requestor;
                request.RequestingNgo = donorNgo;
                _context.Donations.Update(donation);

                _context.Add(request);
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(Index));
            }
            return View(request);
        }

        // GET: Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RequestDate,Quantity,Completed")] Request request)
        {
            if (id != request.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(request);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.Id))
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
            return View(request);
        }

        // GET: Requests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.Requests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var request = await _context.Requests.FindAsync(id);
            _context.Requests.Remove(request);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequestExists(int id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }
    }
}
