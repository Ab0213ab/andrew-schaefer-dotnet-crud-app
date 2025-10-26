using AquentChallenge.Data;
using AquentChallenge.Models;
using AquentChallenge.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

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

        private ClientFormViewModel MapToViewModel(Client client,
            IEnumerable<Person>? allPeople = null,
            List<int>? associatedIds = null,
            bool isReadOnly = false)
        {
            return new ClientFormViewModel
            {
                Id = client.Id,
                CompanyName = client.CompanyName,
                Website = client.Website,
                Phone = client.Phone,
                Address = client.Address,
                AllPeople = allPeople,
                AssociatedIds = associatedIds,
                IsReadOnly = isReadOnly
            };
        }

        private Client MapToEntity(ClientFormViewModel vm)
        {
            return new Client
            {
                Id = vm.Id,
                CompanyName = vm.CompanyName,
                Website = vm.Website,
                Phone = vm.Phone,
                Address = vm.Address
            };
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
            // TODO: Create a ClientIndexViewModel and use it
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
            //
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client == null)
            {
                _logger.LogWarning("Client not found (id={Id})", id);
                return NotFound();
            }

            var contacts = await _context.People
            .Where(p => p.ClientId == client.Id && !p.IsDeleted)
            .ToListAsync();

            var vm = MapToViewModel(client, contacts, contacts.Select(c => c.Id).ToList(), isReadOnly: true);

            _logger.LogDebug("Details for client {Id} returning Form view with {Contacts} contacts", id, contacts.Count);
            ViewData["Title"] = "Client Details";
            return View("Form", vm);
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("GET Clients Create called");

            var vm = new ClientFormViewModel
            {
                AllPeople = await _context.People
                    .Where(p => !p.IsDeleted)
                    .ToListAsync()
            };
            ViewData["Title"] = "Client Create";
            return View("Form", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientFormViewModel vm)
        {
            _logger.LogInformation("POST Clients Create called for CompanyName={Company}", vm.CompanyName);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Create ModelState invalid for CompanyName={Company}", vm.CompanyName);
                vm.AllPeople = await _context.People
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();
                return View("Form", vm);
            }

            try 
            {
                var client = MapToEntity(vm);
                _context.Add(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created client {ClientId} ({Company})", client.Id, client.CompanyName);

                TempData["ToastMessage"] = "Client created successfully!";
                TempData["ToastType"] = "success";

                return RedirectToAction(nameof(Index));
            }
            // Global error handling is already configured in Program.cs, but I want to specifically
            // log update exceptions here to showcase enterprise level error handling.
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating client {Company}", vm.CompanyName);
                return View("Form", vm);
            }
            
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
            var allPeople = await _context.People
                .Where(p => !p.IsDeleted && (p.ClientId == null || p.ClientId == client.Id))
                .ToListAsync();

            // Used to pre-checked associated UI checkboxes
            var associatedIds = allPeople
                .Where(p => p.ClientId == client.Id)
                .Select(p => p.Id)
                .ToList();

            var vm = MapToViewModel(client, allPeople, associatedIds);
            ViewData["Title"] = "Client Edit";
            return View("Form", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientFormViewModel vm, int[] SelectedPeople)
        {
            _logger.LogInformation("POST Clients Edit called (id={Id})", id);

            if (id != vm.Id)
            {
                _logger.LogWarning("Edit id mismatch (route id={RouteId} body id={BodyId})", id, vm.Id);
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Edit ModelState invalid for client {Id}", vm.Id);
                vm.AllPeople = await _context.People
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();
                ViewData["Title"] = "Client Edit";
                return View("Form", vm);
            }

            var client = MapToEntity(vm);

            try
            {
                // Update the client record first
                _context.Update(client);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated client {Id}", client.Id);
                TempData["ToastMessage"] = "Client updated successfully!";
                TempData["ToastType"] = "info";

            }
            // Global error handling is already configured in Program.cs, but I want to specifically
            // log concurrency exceptions here to showcase enterprise level error handling.
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency exception updating person {Id}", client.Id);
                if (!_context.People.Any(e => e.Id == client.Id))
                ViewData["Title"] = "Client Edit";
                return NotFound();
                throw;

            }

            var allowedPeople = await _context.People
                .Where(p => !p.IsDeleted && (p.ClientId == null || p.ClientId == client.Id))
                .ToListAsync();

            SelectedPeople ??= Array.Empty<int>();

            foreach (var person in allowedPeople)
            {
                var before = person.ClientId;
                person.ClientId = SelectedPeople.Contains(person.Id) ? client.Id : (int?)null;

                if (before != person.ClientId)
                {
                    _logger.LogInformation(
                        "Person {PersonId} ClientId changed {Before} -> {After}",
                        person.Id,
                        before?.ToString() ?? "null",
                        person.ClientId?.ToString() ?? "null");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated client {Id}", client.Id);

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
                TempData["ToastMessage"] = "Client deleted successfully!";
                TempData["ToastType"] = "danger";
            }
            else
            {
                _logger.LogWarning("DeleteConfirmed could not find client {Id}", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
