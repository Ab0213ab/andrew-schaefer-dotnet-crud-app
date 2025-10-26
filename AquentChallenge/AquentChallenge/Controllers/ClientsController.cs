using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AquentChallenge.Data;
using AquentChallenge.Models;

namespace AquentChallenge.Controllers
{
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(AppDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            _logger.LogInformation("GET Clients Index called (showDeleted={ShowDeleted})", showDeleted);
            // If showDeleted == false hide soft-deleted clients; toggle controlled by UI/query string.
            IQueryable<Client> query = _context.Clients;

            if (!showDeleted)
            {
                query = query.Where(c => !c.IsDeleted);
            }

            var clients = await query.ToListAsync();
            _logger.LogDebug("Index returned {Count} clients", clients.Count);
            // TODO: Add a toggle button in the UI to control this
            ViewData["ShowDeleted"] = showDeleted;

            return View(clients);
        }


        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("GET Clients Details called (id={Id})", id);

            if (id == null)
            {
                _logger.LogWarning("Details called with null id");
                return NotFound();
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
            {
                _logger.LogWarning("Client not found (id={Id})", id);
                return NotFound();
            }

            var contacts = await _context.People
            .Where(p => p.ClientId == client.Id && !p.IsDeleted)
            .ToListAsync();

            ViewData["AllPeople"] = contacts;
            ViewData["AssociatedIds"] = contacts.Select(c => c.Id).ToList();
            // Reuse the "Form" view for a read-only details page
            ViewData["IsReadOnly"] = true;

            _logger.LogDebug("Details for client {Id} returning Form view with {Contacts} contacts", id, contacts.Count);
            return View("Form", client);
        }


        public IActionResult Create()
        {
            _logger.LogInformation("GET Clients Create called");
            return View("Form", new Client());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyName,Website,Phone,Address")] Client client)
        {
            _logger.LogInformation("POST Clients Create called for CompanyName={Company}", client.CompanyName);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Create ModelState invalid for CompanyName={Company}", client.CompanyName);
                return View("Form", client);
            }   

            _context.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created client {ClientId} ({Company})", client.Id, client.CompanyName);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("GET Clients Edit called (id={Id})", id);

            if (id == null)
            {
                _logger.LogWarning("Edit called with null id");
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                _logger.LogWarning("Client not found for Edit (id={Id})", id);
                return NotFound();
            }

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
            _logger.LogInformation("POST Clients Edit called (id={Id}) SelectedPeopleCount={Count}", id, SelectedPeople?.Length ?? 0);

            if (id != client.Id)
            {
                _logger.LogWarning("Edit id mismatch (route id={RouteId} body id={BodyId})", id, client.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Edit ModelState invalid for client {Id}", client.Id);
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
                _logger.LogInformation("Updated client {Id}", client.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency exception updating client {Id}", client.Id);
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
            _logger.LogDebug("AllowedPeople={AllowedCount}, SelectedPeople={SelectedCount}", allowedPeople.Count, SelectedPeople.Length);

            foreach (var person in allowedPeople)
            {
                var before = person.ClientId;
                person.ClientId = SelectedPeople.Contains(person.Id) ? client.Id : (int?)null;
                if (before != person.ClientId)
                {
                    _logger.LogInformation("Person {PersonId} ClientId changed {Before} -> {After}", person.Id, before?.ToString() ?? "null", person.ClientId?.ToString() ?? "null");
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // "Soft" delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("POST Clients DeleteConfirmed called (id={Id})", id);
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                client.IsDeleted = true;
                _context.Update(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Soft-deleted client {Id}", id);
            }
            else
            {
                _logger.LogWarning("DeleteConfirmed could not find client {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
