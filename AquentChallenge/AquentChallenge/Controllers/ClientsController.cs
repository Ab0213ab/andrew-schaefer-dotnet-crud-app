using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AquentChallenge.Data;
using AquentChallenge.Models;

namespace AquentChallenge.Controllers
{
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;

        public ClientsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Where(c => !c.IsDeleted)
                .ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            ViewData["IsReadOnly"] = true;
            return View("Form", client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View("Form", new Client());
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyName,Website,Phone,Address")] Client client)
        {
            if (!ModelState.IsValid)
                return View("Form", client);

            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            return View("Form", client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,Website,Phone,Address")] Client client)
        {
            if (id != client.Id) return NotFound();

            if (!ModelState.IsValid)
                return View("Form", client);

            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.Id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            return View(client); // TODO: Replace this with a modal later
        }

        // POST: Clients/Delete/5 (soft delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = true;
                _context.Update(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
