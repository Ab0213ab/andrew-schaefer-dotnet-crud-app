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


        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            // If showDeleted == false hide soft-deleted clients; toggle controlled by UI/query string.
            IQueryable<Client> query = _context.Clients;

            if (!showDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            var clients = await query.ToListAsync();
            // TODO: Add a toggle button in the UI to control this
            ViewData["ShowDeleted"] = showDeleted;

            return View(clients);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            var contacts = await _context.People
            .Where(p => p.ClientId == client.Id && !p.IsDeleted)
            .ToListAsync();

            ViewData["AllPeople"] = contacts;
            ViewData["AssociatedIds"] = contacts.Select(c => c.Id).ToList();
            // Reuse the "Form" view for a read-only details page
            ViewData["IsReadOnly"] = true;
            return View("Form", client);
        }


        public IActionResult Create()
        {
            return View("Form", new Client());
        }


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


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            // We only want unassigned contacts so they can be assigned or already
            // assigned contacts so they can be unassigned from this Client
            ViewData["AllPeople"] = await _context.People
            .Where(p => !p.IsDeleted && (p.ClientId == null || p.ClientId == client.Id))
            .ToListAsync();

            // Used to pre-checked associated UI checkboxes
            ViewData["AssociatedIds"] = await _context.People
            .Where(p => !p.IsDeleted && p.ClientId == client.Id)
            .Select(p => p.Id)
            .ToListAsync();

            return View("Form", client);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id
            , [Bind("Id,CompanyName,Website,Phone,Address")] Client client
            , int[] SelectedPeople
            )
        {
            if (id != client.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Rebuild view data so page can re-render with errors

                // We only want unassigned contacts so they can be assigned or already
                // assigned contacts so they can be unassigned from this Client
                ViewData["AllPeople"] = await _context.People
                    .Where(p => !p.IsDeleted && (p.ClientId == null || p.ClientId == client.Id))
                    .ToListAsync();

                // Used to pre-checked associated UI checkboxes
                ViewData["AssociatedIds"] = await _context.People
                    .Where(p => !p.IsDeleted && p.ClientId == client.Id)
                    .Select(p => p.Id)
                    .ToListAsync();

                ViewData["IsReadOnly"] = false;
                return View("Form", client);
            }

            try
            {
                // Update the client record first
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.Id)) return NotFound();
                throw;
            }

            // Safeguard: Only update people that are not deleted, and are either
            // unassigned or already assigned to this client. This prevents
            // users from 'stealing' contacts assigned to other clients
            var allowedPeople = await _context.People
                .Where(p => !p.IsDeleted && (p.ClientId == null || p.ClientId == client.Id))
                .ToListAsync();

            // For when no checkboxes are checked
            SelectedPeople ??= Array.Empty<int>();

            foreach (var person in allowedPeople)
            {
                person.ClientId = SelectedPeople.Contains(person.Id) ? client.Id : (int?)null;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null) return NotFound();

            // TODO: Replace this with a modal later
            return View(client);
        }


        // Soft delete
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
