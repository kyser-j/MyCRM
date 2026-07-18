using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class PeopleController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q)
    {
        var query = db.People.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.FirstName.Contains(term) ||
                (p.LastName != null && p.LastName.Contains(term)));
        }

        ViewBag.Search = q;

        var people = await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        return View(people);
    }

    public async Task<IActionResult> Details(long id)
    {
        var person = await db.People
            .AsNoTracking()
            .Include(p => p.CompanyPeople).ThenInclude(cp => cp.Company)
            .Include(p => p.CompanyPeople).ThenInclude(cp => cp.CompanyPersonRoles).ThenInclude(r => r.CompanyRole)
            .Include(p => p.EmailAddresses)
            .Include(p => p.PhoneNumbers)
            .Include(p => p.WebLinks)
            .Include(p => p.Notes)
            .Include(p => p.FollowUps)
            .Include(p => p.CorrespondenceRecords)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (person is null)
            return NotFound();

        return View(person);
    }

    public IActionResult Create() => View(new Person { FirstName = string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person model)
    {
        if (string.IsNullOrWhiteSpace(model.FirstName))
            ModelState.AddModelError(nameof(model.FirstName), "First name is required.");

        if (!ModelState.IsValid)
            return View(model);

        var person = new Person
        {
            FirstName = model.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(model.LastName) ? null : model.LastName.Trim()
        };
        CrmHelpers.StampCreate(person);

        db.People.Add(person);
        await db.SaveChangesAsync();

        TempData["Success"] = "Person created.";
        return RedirectToAction(nameof(Details), new { id = person.Id });
    }

    public async Task<IActionResult> Edit(long id)
    {
        var person = await db.People.FindAsync(id);
        if (person is null)
            return NotFound();

        return View(person);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Person model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.FirstName))
            ModelState.AddModelError(nameof(model.FirstName), "First name is required.");

        if (!ModelState.IsValid)
            return View(model);

        var person = await db.People.FindAsync(id);
        if (person is null)
            return NotFound();

        person.FirstName = model.FirstName.Trim();
        person.LastName = string.IsNullOrWhiteSpace(model.LastName) ? null : model.LastName.Trim();
        CrmHelpers.StampUpdate(person);

        await db.SaveChangesAsync();
        TempData["Success"] = "Person updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(long id)
    {
        var person = await db.People.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (person is null)
            return NotFound();

        return View(person);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var person = await db.People.FindAsync(id);
        if (person is null)
            return NotFound();

        db.People.Remove(person);
        await db.SaveChangesAsync();

        TempData["Success"] = "Person deleted.";
        return RedirectToAction(nameof(Index));
    }
}
