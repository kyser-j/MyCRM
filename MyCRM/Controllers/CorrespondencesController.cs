using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Constants;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class CorrespondencesController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await db.Correspondences
            .AsNoTracking()
            .Include(c => c.Company)
            .Include(c => c.Person)
            .OrderByDescending(c => c.OccurredAt)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Details(long id)
    {
        var item = await db.Correspondences
            .AsNoTracking()
            .Include(c => c.Company)
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id);

        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        await PopulateLookups(companyId, personId);
        return View(new Correspondence
        {
            OccurredAt = DateTimeOffset.Now,
            Type = CorrespondenceType.Email,
            Direction = Direction.Outbound,
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Correspondence model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var item = new Correspondence
        {
            Subject = NullIfEmpty(model.Subject),
            Content = NullIfEmpty(model.Content),
            Type = model.Type,
            Direction = model.Direction,
            OccurredAt = model.OccurredAt,
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(item);

        db.Correspondences.Add(item);
        await db.SaveChangesAsync();

        TempData["Success"] = "Correspondence logged.";
        return RedirectToAction(nameof(Details), new { id = item.Id });
    }

    public async Task<IActionResult> Edit(long id)
    {
        var item = await db.Correspondences.FindAsync(id);
        if (item is null)
            return NotFound();

        await PopulateLookups(item.CompanyId, item.PersonId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Correspondence model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var item = await db.Correspondences.FindAsync(id);
        if (item is null)
            return NotFound();

        item.Subject = NullIfEmpty(model.Subject);
        item.Content = NullIfEmpty(model.Content);
        item.Type = model.Type;
        item.Direction = model.Direction;
        item.OccurredAt = model.OccurredAt;
        item.CompanyId = model.CompanyId;
        item.PersonId = model.PersonId;
        CrmHelpers.StampUpdate(item);

        await db.SaveChangesAsync();
        TempData["Success"] = "Correspondence updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(long id)
    {
        var item = await db.Correspondences
            .AsNoTracking()
            .Include(c => c.Company)
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id);

        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var item = await db.Correspondences.FindAsync(id);
        if (item is null)
            return NotFound();

        db.Correspondences.Remove(item);
        await db.SaveChangesAsync();

        TempData["Success"] = "Correspondence deleted.";
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
        ViewBag.Types = CrmHelpers.EnumSelectList<CorrespondenceType>();
        ViewBag.Directions = CrmHelpers.EnumSelectList<Direction>();
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
