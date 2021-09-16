using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(this.User);
            List<Request> requests = null;
            try
            {
                if (await _userManager.IsInRoleAsync(user, "Ngo"))
                {
                    requests = await _context.Requests.Include(d => d.Medicine).Include(d => d.Requestor).Where(d => d.RequestingNgo.Id == user.Id).ToListAsync();
                }
                else
                {
                    requests = await _context.Requests.Include(d => d.Medicine).Where(d => d.Requestor.Id == user.Id).ToListAsync();
                }
                return View(requests);
            }
            catch (Exception)
            {
                return View();
            }
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

        public async Task<IActionResult> Invoice(int? id)
        {
            /*int? requestId = Convert.ToInt32(HttpContext.Session.GetString("requestId"));
            if (id == null || requestId == null || requestId != id)
            {
                return NotFound();
            }*/
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(this.User);
            var request = await _context.Requests
                .Include(r => r.Requestor)
                .Include(r => r.Medicine)
                .Include(r => r.RequestingNgo)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                HttpContext.Session.Remove("requestId");
                return NotFound();
            }

            HttpContext.Session.Remove("requestId");
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

                HttpContext.Session.SetString("requestId", request.Id.ToString());

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

        public async Task<IActionResult> Search(string q, string page)
        {
            var pages = new List<string>() //ya unnecessary memory consumption
            {
                "Index", "MyRequests"
            };
            if (string.IsNullOrEmpty(page) || !pages.Contains(page))
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(q))
            {
                return RedirectToAction(page);
            }

            q = q.ToLower();
            var user = await _userManager.GetUserAsync(this.User);
            IEnumerable<Request> allRequests = null;
            IEnumerable<Request> requests = null;

            //yea i know its cheating, but upaya tha xaina
            if (page.Equals("MyRequests"))
            {
                allRequests = await _context.Requests.Include(d => d.Medicine).Where(d => d.Requestor.Id == user.Id).ToListAsync();
            }
            else if (page.Equals("Index"))
            {
                allRequests = await _context.Requests.Include(d => d.Medicine).Include(d => d.Requestor).Include(d => d.RequestingNgo).ToListAsync();
            }
            else if (page.Equals("DonatedMedicines"))
            {
                allRequests = await _context.Requests.Include(d => d.Medicine).Where(d => d.Completed).ToListAsync();
            }
            try
            {
                requests = allRequests
                    .Where(d => d.Medicine.Name.ToLower().Contains(q) ||
                        d.Medicine.GenericName.ToLower().Contains(q) ||
                        d.Medicine.Description.ToLower().Contains(q));
                if (!requests.Any())
                {
                    return View(page, allRequests);
                }
                return View(page, requests);
            }
            catch (Exception)
            {

                return View(page, allRequests);
            }
        }
    }
}
