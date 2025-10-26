using AquentChallenge.Data;
using AquentChallenge.Models;
using AquentChallenge.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquentChallenge.Controllers
{
    public class PeopleController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public PeopleController(AppDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private PersonFormViewModel MapToViewModel(Person person, IEnumerable<Client>? clients = null, bool isReadOnly = false, string? clientName = null)
        {
            return new PersonFormViewModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                EmailAddress = person.EmailAddress,
                StreetAddress = person.StreetAddress,
                City = person.City,
                State = person.State,
                ZipCode = person.ZipCode,
                ClientId = person.ClientId,
                Clients = clients,
                IsReadOnly = isReadOnly,
                ClientName = clientName
            };
        }

        private Person MapToEntity(PersonFormViewModel vm)
        {
            return new Person
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                EmailAddress = vm.EmailAddress,
                StreetAddress = vm.StreetAddress,
                City = vm.City,
                State = vm.State,
                ZipCode = vm.ZipCode,
                ClientId = vm.ClientId
            };
        }


        public async Task<IActionResult> Index(bool showDeleted = false)
        {
            _logger.LogInformation("GET People Index called (showDeleted={ShowDeleted})", showDeleted);
            IQueryable<Person> query = _context.People;

            if (!showDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

          var people = await (from p in _context.People
                                where !p.IsDeleted
                                join c in _context.Clients
                                    on p.ClientId equals c.Id into pc
                                from client in pc.DefaultIfEmpty()
                                select new
                                {
                                    p.Id,
                                    p.FirstName,
                                    p.LastName,
                                    p.EmailAddress,
                                    p.StreetAddress,
                                    p.City,
                                    p.State,
                                    p.ZipCode,
                                    ClientName = client != null ? client.CompanyName : "(none)",
                                    p.IsDeleted
                                })
                        .ToListAsync();
            _logger.LogDebug("Index returned {Count} people", people.Count);
            // TODO: Create a view model for this instead of using anonymous types
            ViewData["People"] = people;
            // TODO: Add a toggle button in the UI to control this
            ViewData["ShowDeleted"] = showDeleted;

            return View(people);
        }


        public async Task<IActionResult> Details(int? id)
        {
            _logger.LogInformation("GET Person Details called (id={Id})", id);

            if (id == null)
            {
                _logger.LogWarning("Details called with null id");
                return NotFound();
            }

            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                _logger.LogWarning("Person not found (id={Id})", id);
                return NotFound();
            }

            var clientName = await _context.Clients
            .Where(c => c.Id == person.ClientId)
            .Select(c => c.CompanyName)
            .FirstOrDefaultAsync() ?? "(none)";

            var vm = MapToViewModel(person, null, isReadOnly: true, clientName: clientName);
            return View("Form", vm);
        }


        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("GET People Create called");
            var clients = await _context.Clients
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var vm = new PersonFormViewModel { Clients = clients };
            return View("Form", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonFormViewModel vm)
        {
            var name = $"{vm.FirstName} {vm.LastName}";
            _logger.LogInformation("POST People Create called for Name={Name}", name);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Create ModelState invalid for Name={Name}", name);
                vm.Clients = await _context.Clients
                    .Where(c => !c.IsDeleted)
                    .ToListAsync();
                return View("Form", vm);
            }

            var person = MapToEntity(vm);
            _context.Add(person);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created person {person.Id} ({name})", person.Id, name);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("GET People Edit called (id={Id})", id);

            if (id == null)
            {
                _logger.LogWarning("Edit called with null id");
                return NotFound();
            }

            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                _logger.LogWarning("Client not found for Edit (id={Id})", id);
                return NotFound();
            }

            var clients = await _context.Clients
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var vm = MapToViewModel(person, clients);
            return View("Form", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PersonFormViewModel vm)
        {
            _logger.LogInformation("POST People Edit called (id={Id})", id);

            if (id != vm.Id)
            {
                _logger.LogWarning("Edit id mismatch (route id={RouteId} body id={BodyId})", id, vm.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Edit ModelState invalid for client {Id}", vm.Id);
                vm.Clients = await _context.Clients
                    .Where(c => !c.IsDeleted)
                    .ToListAsync();
                return View("Form", vm);
            }

            var person = MapToEntity(vm);

            try
            {
                _context.Update(person);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated person {Id}", person.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency exception updating person {Id}", person.Id);
                if (!_context.People.Any(e => e.Id == person.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }


        // "Soft" delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("POST People DeleteConfirmed called (id={Id})", id);
            var person = await _context.People.FindAsync(id);
            if (person != null)
            {
                person.IsDeleted = true;
                _context.Update(person);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Soft-deleted person {Id}", id);
            }
            else
            {
                _logger.LogWarning("DeleteConfirmed could not find person {Id}", id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
