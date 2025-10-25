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

        // GET: People
        public async Task<IActionResult> Index()
        {
            var people = await _context.People
                .Where(p => !p.IsDeleted)
                .ToListAsync();
            return View(people);
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return NotFound();

            ViewData["IsReadOnly"] = true;
            return View("Form", person);
        }

        public IActionResult Create()
        {
            return View("Form", new Person());
        }


        // POST: People/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person)
        {
            if (!ModelState.IsValid)
                return View("Form", person);

            _context.Add(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FindAsync(id);
            if (person == null) return NotFound();

            return View("Form", person);
        }

        // POST: People/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,EmailAddress,StreetAddress,City,State,ZipCode,ClientId")] Person person)
        {
            if (id != person.Id) return NotFound();

            if (!ModelState.IsValid)
                return View("Form", person);

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

        // GET: People/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return NotFound();

            return View(person); // TODO: replace this with modal later
        }

        // POST: People/Delete/5 (soft delete)
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
