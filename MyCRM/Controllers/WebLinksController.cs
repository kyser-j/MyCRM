using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class WebLinksController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        if (companyId is null && personId is null)
            return BadRequest("Company or person is required.");

        await PopulateLookups(companyId, personId);
        return View(new WebLink
        {
            Link = string.Empty,
            Description = string.Empty,
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WebLink model)
    {
        if (string.IsNullOrWhiteSpace(model.Link))
            ModelState.AddModelError(nameof(model.Link), "Link is required.");
        if (string.IsNullOrWhiteSpace(model.Description))
            ModelState.AddModelError(nameof(model.Description), "Description is required.");
        if (model.CompanyId is null && model.PersonId is null)
            ModelState.AddModelError(string.Empty, "Attach to a company or person.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var link = new WebLink
        {
            Link = model.Link.Trim(),
            Description = model.Description.Trim(),
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(link);

        db.WebLinks.Add(link);
        await db.SaveChangesAsync();

        TempData["Success"] = "Link added.";
        return RedirectToParent(link.CompanyId, link.PersonId);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var link = await db.WebLinks.FindAsync(id);
        if (link is null)
            return NotFound();

        await PopulateLookups(link.CompanyId, link.PersonId);
        return View(link);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, WebLink model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Link))
            ModelState.AddModelError(nameof(model.Link), "Link is required.");
        if (string.IsNullOrWhiteSpace(model.Description))
            ModelState.AddModelError(nameof(model.Description), "Description is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var link = await db.WebLinks.FindAsync(id);
        if (link is null)
            return NotFound();

        link.Link = model.Link.Trim();
        link.Description = model.Description.Trim();
        link.CompanyId = model.CompanyId;
        link.PersonId = model.PersonId;
        CrmHelpers.StampUpdate(link);

        await db.SaveChangesAsync();
        TempData["Success"] = "Link updated.";
        return RedirectToParent(link.CompanyId, link.PersonId);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var link = await db.WebLinks
            .AsNoTracking()
            .Include(w => w.Company)
            .Include(w => w.Person)
            .FirstOrDefaultAsync(w => w.Id == id);

        return link is null ? NotFound() : View(link);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var link = await db.WebLinks.FindAsync(id);
        if (link is null)
            return NotFound();

        var companyId = link.CompanyId;
        var personId = link.PersonId;

        db.WebLinks.Remove(link);
        await db.SaveChangesAsync();

        TempData["Success"] = "Link deleted.";
        return RedirectToParent(companyId, personId);
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

    private IActionResult RedirectToParent(long? companyId, long? personId)
    {
        if (companyId.HasValue)
            return RedirectToAction("Details", "Companies", new { id = companyId });
        if (personId.HasValue)
            return RedirectToAction("Details", "People", new { id = personId });
        return RedirectToAction("Index", "Home");
    }
}
