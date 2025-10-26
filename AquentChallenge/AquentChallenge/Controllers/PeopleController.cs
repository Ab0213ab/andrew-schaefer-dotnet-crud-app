using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AquentChallenge.Data;
using AquentChallenge.Models;

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


        private void LoadClients()
        {
            ViewData["Clients"] = _context.Clients
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Id, c.CompanyName })
                .ToList();
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

            var client = await _context.Clients
                .Where(c => c.Id == person.ClientId)
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync();

            ViewData["ClientName"] = client ?? "(none)";
            ViewData["IsReadOnly"] = true;

            return View("Form", person);
        }

        public IActionResult Create()
        {
            _logger.LogInformation("GET People Create called");
            LoadClients();
            return View("Form", new Person());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person)
        {
            var name = $"{person.FirstName} {person.LastName}";
            _logger.LogInformation("POST People Create called for Name={Name}", name);

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Create ModelState invalid for Name={Name}", name);
                LoadClients();
                return View("Form", person);
            }

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

            LoadClients();
            return View("Form", person);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id
            , [Bind("Id,FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person
            )
        {
            _logger.LogInformation("POST People Edit called (id={Id})", id);

            if (id != person.Id)
            {
                _logger.LogWarning("Edit id mismatch (route id={RouteId} body id={BodyId})", id, person.Id);
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Edit ModelState invalid for client {Id}", person.Id);
                LoadClients();
                return View("Form", person);
            }

            try
            {
                _context.Update(person);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated person {Id}", person.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency exception updating person {Id}", person.Id);
                if (!PersonExists(person.Id)) return NotFound();
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


        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
