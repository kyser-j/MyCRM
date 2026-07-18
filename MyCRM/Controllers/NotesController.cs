using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class NotesController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var notes = await db.Notes
            .AsNoTracking()
            .Include(n => n.Company)
            .Include(n => n.Person)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return View(notes);
    }

    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        await PopulateLookups(companyId, personId);
        return View(new Note
        {
            Content = string.Empty,
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Note model)
    {
        if (string.IsNullOrWhiteSpace(model.Content))
            ModelState.AddModelError(nameof(model.Content), "Content is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var note = new Note
        {
            Content = model.Content.Trim(),
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(note);

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        TempData["Success"] = "Note added.";
        return RedirectToParentOrIndex(note);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return NotFound();

        await PopulateLookups(note.CompanyId, note.PersonId);
        return View(note);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Note model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Content))
            ModelState.AddModelError(nameof(model.Content), "Content is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return NotFound();

        note.Content = model.Content.Trim();
        note.CompanyId = model.CompanyId;
        note.PersonId = model.PersonId;
        CrmHelpers.StampUpdate(note);

        await db.SaveChangesAsync();
        TempData["Success"] = "Note updated.";
        return RedirectToParentOrIndex(note);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var note = await db.Notes
            .AsNoTracking()
            .Include(n => n.Company)
            .Include(n => n.Person)
            .FirstOrDefaultAsync(n => n.Id == id);

        return note is null ? NotFound() : View(note);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null)
            return NotFound();

        var companyId = note.CompanyId;
        var personId = note.PersonId;

        db.Notes.Remove(note);
        await db.SaveChangesAsync();

        TempData["Success"] = "Note deleted.";

        if (companyId.HasValue)
            return RedirectToAction("Details", "Companies", new { id = companyId });
        if (personId.HasValue)
            return RedirectToAction("Details", "People", new { id = personId });
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookups(long? companyId, long? personId)
    {
        ViewBag.Companies = new SelectList(
            await db.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", companyId);
        ViewBag.People = new SelectList(
            await db.People.AsNoTracking()
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Select(p => new { p.Id, Name = p.LastName == null ? p.FirstName : p.LastName + ", " + p.FirstName })
                .ToListAsync(),
            "Id", "Name", personId);
    }

    private IActionResult RedirectToParentOrIndex(Note note)
    {
        if (note.CompanyId.HasValue)
            return RedirectToAction("Details", "Companies", new { id = note.CompanyId });
        if (note.PersonId.HasValue)
            return RedirectToAction("Details", "People", new { id = note.PersonId });
        return RedirectToAction(nameof(Index));
    }
}
