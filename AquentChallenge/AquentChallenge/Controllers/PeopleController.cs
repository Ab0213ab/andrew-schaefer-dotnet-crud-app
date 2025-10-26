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

        public PeopleController(AppDbContext context)
        {
            _context = context;
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
            // TODO: Create a view model for this instead of using anonymous types
            ViewData["People"] = people;
            // TODO: Add a toggle button in the UI to control this
            ViewData["ShowDeleted"] = showDeleted;

            return View(people);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return NotFound();

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
            LoadClients();
            return View("Form", new Person());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person)
        {
            if (!ModelState.IsValid)
            {
                LoadClients();
                return View("Form", person);
            }

            _context.Add(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            LoadClients();
            return View("Form", person);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person)
        {
            if (id != person.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                LoadClients();
                return View("Form", person);
            }

            try
            {
                _context.Update(person);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
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
            var person = await _context.People.FindAsync(id);
            if (person != null)
            {
                person.IsDeleted = true;
                _context.Update(person);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
