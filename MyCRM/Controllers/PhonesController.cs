using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class PhonesController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        if (companyId is null && personId is null)
            return BadRequest("Company or person is required.");

        await PopulateLookups(companyId, personId);
        return View(new Phone
        {
            PhoneNumber = string.Empty,
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Phone model)
    {
        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required.");
        if (model.CompanyId is null && model.PersonId is null)
            ModelState.AddModelError(string.Empty, "Attach to a company or person.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        if (model.IsPrimary)
            await ClearPrimaryAsync(model.CompanyId, model.PersonId);

        var phone = new Phone
        {
            PhoneNumber = model.PhoneNumber.Trim(),
            IsPrimary = model.IsPrimary,
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(phone);

        db.Phones.Add(phone);
        await db.SaveChangesAsync();

        TempData["Success"] = "Phone added.";
        return RedirectToParent(phone.CompanyId, phone.PersonId);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var phone = await db.Phones.FindAsync(id);
        if (phone is null)
            return NotFound();

        await PopulateLookups(phone.CompanyId, phone.PersonId);
        return View(phone);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Phone model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var phone = await db.Phones.FindAsync(id);
        if (phone is null)
            return NotFound();

        if (model.IsPrimary)
            await ClearPrimaryAsync(model.CompanyId, model.PersonId, id);

        phone.PhoneNumber = model.PhoneNumber.Trim();
        phone.IsPrimary = model.IsPrimary;
        phone.CompanyId = model.CompanyId;
        phone.PersonId = model.PersonId;
        CrmHelpers.StampUpdate(phone);

        await db.SaveChangesAsync();
        TempData["Success"] = "Phone updated.";
        return RedirectToParent(phone.CompanyId, phone.PersonId);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var phone = await db.Phones
            .AsNoTracking()
            .Include(p => p.Company)
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        return phone is null ? NotFound() : View(phone);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var phone = await db.Phones.FindAsync(id);
        if (phone is null)
            return NotFound();

        var companyId = phone.CompanyId;
        var personId = phone.PersonId;

        db.Phones.Remove(phone);
        await db.SaveChangesAsync();

        TempData["Success"] = "Phone deleted.";
        return RedirectToParent(companyId, personId);
    }

    private async Task ClearPrimaryAsync(long? companyId, long? personId, long? exceptId = null)
    {
        if (companyId.HasValue)
        {
            var others = await db.Phones
                .Where(p => p.CompanyId == companyId && p.IsPrimary && p.Id != exceptId)
                .ToListAsync();
            foreach (var p in others)
                p.IsPrimary = false;
        }

        if (personId.HasValue)
        {
            var others = await db.Phones
                .Where(p => p.PersonId == personId && p.IsPrimary && p.Id != exceptId)
                .ToListAsync();
            foreach (var p in others)
                p.IsPrimary = false;
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
