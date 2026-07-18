using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class EmailsController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        if (companyId is null && personId is null)
            return BadRequest("Company or person is required.");

        await PopulateLookups(companyId, personId);
        return View(new Email
        {
            EmailAddress = string.Empty,
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Email model)
    {
        if (string.IsNullOrWhiteSpace(model.EmailAddress))
            ModelState.AddModelError(nameof(model.EmailAddress), "Email is required.");
        if (model.CompanyId is null && model.PersonId is null)
            ModelState.AddModelError(string.Empty, "Attach to a company or person.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        if (model.IsPrimary)
            await ClearPrimaryAsync(model.CompanyId, model.PersonId);

        var email = new Email
        {
            EmailAddress = model.EmailAddress.Trim(),
            IsPrimary = model.IsPrimary,
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(email);

        db.Emails.Add(email);
        await db.SaveChangesAsync();

        TempData["Success"] = "Email added.";
        return RedirectToParent(email.CompanyId, email.PersonId);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var email = await db.Emails.FindAsync(id);
        if (email is null)
            return NotFound();

        await PopulateLookups(email.CompanyId, email.PersonId);
        return View(email);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Email model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.EmailAddress))
            ModelState.AddModelError(nameof(model.EmailAddress), "Email is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var email = await db.Emails.FindAsync(id);
        if (email is null)
            return NotFound();

        if (model.IsPrimary)
            await ClearPrimaryAsync(model.CompanyId, model.PersonId, id);

        email.EmailAddress = model.EmailAddress.Trim();
        email.IsPrimary = model.IsPrimary;
        email.CompanyId = model.CompanyId;
        email.PersonId = model.PersonId;
        CrmHelpers.StampUpdate(email);

        await db.SaveChangesAsync();
        TempData["Success"] = "Email updated.";
        return RedirectToParent(email.CompanyId, email.PersonId);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var email = await db.Emails
            .AsNoTracking()
            .Include(e => e.Company)
            .Include(e => e.Person)
            .FirstOrDefaultAsync(e => e.Id == id);

        return email is null ? NotFound() : View(email);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var email = await db.Emails.FindAsync(id);
        if (email is null)
            return NotFound();

        var companyId = email.CompanyId;
        var personId = email.PersonId;

        db.Emails.Remove(email);
        await db.SaveChangesAsync();

        TempData["Success"] = "Email deleted.";
        return RedirectToParent(companyId, personId);
    }

    private async Task ClearPrimaryAsync(long? companyId, long? personId, long? exceptId = null)
    {
        if (companyId.HasValue)
        {
            var others = await db.Emails
                .Where(e => e.CompanyId == companyId && e.IsPrimary && e.Id != exceptId)
                .ToListAsync();
            foreach (var e in others)
                e.IsPrimary = false;
        }

        if (personId.HasValue)
        {
            var others = await db.Emails
                .Where(e => e.PersonId == personId && e.IsPrimary && e.Id != exceptId)
                .ToListAsync();
            foreach (var e in others)
                e.IsPrimary = false;
        }
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
