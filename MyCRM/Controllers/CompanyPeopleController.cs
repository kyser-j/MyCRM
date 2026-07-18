using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class CompanyPeopleController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        if (companyId is null && personId is null)
            return BadRequest("Company or person is required.");

        await PopulateLookups(companyId, personId);
        return View(new CompanyPerson
        {
            CompanyId = companyId ?? 0,
            PersonId = personId ?? 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyPerson model, long[]? roleIds)
    {
        if (model.CompanyId <= 0)
            ModelState.AddModelError(nameof(model.CompanyId), "Company is required.");
        if (model.PersonId <= 0)
            ModelState.AddModelError(nameof(model.PersonId), "Person is required.");

        if (model.CompanyId > 0 && model.PersonId > 0 &&
            await db.CompanyPeople.AnyAsync(cp => cp.CompanyId == model.CompanyId && cp.PersonId == model.PersonId))
        {
            ModelState.AddModelError(string.Empty, "This person is already linked to that company.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        if (model.IsPrimaryContact)
            await ClearPrimaryAsync(model.CompanyId);

        var link = new CompanyPerson
        {
            CompanyId = model.CompanyId,
            PersonId = model.PersonId,
            IsPrimaryContact = model.IsPrimaryContact
        };
        CrmHelpers.StampCreate(link);
        db.CompanyPeople.Add(link);
        await db.SaveChangesAsync();

        await SetRolesAsync(link.Id, roleIds);

        TempData["Success"] = "Person linked to company.";
        return RedirectToAction("Details", "Companies", new { id = link.CompanyId });
    }

    public async Task<IActionResult> Edit(long id)
    {
        var link = await db.CompanyPeople
            .Include(cp => cp.Company)
            .Include(cp => cp.Person)
            .Include(cp => cp.CompanyPersonRoles)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        if (link is null)
            return NotFound();

        ViewBag.RoleIds = link.CompanyPersonRoles.Select(r => r.CompanyRoleId).ToArray();
        await PopulateRoles();
        return View(link);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CompanyPerson model, long[]? roleIds)
    {
        if (id != model.Id)
            return BadRequest();

        var link = await db.CompanyPeople
            .Include(cp => cp.CompanyPersonRoles)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        if (link is null)
            return NotFound();

        if (model.IsPrimaryContact)
            await ClearPrimaryAsync(link.CompanyId, id);

        link.IsPrimaryContact = model.IsPrimaryContact;
        CrmHelpers.StampUpdate(link);
        await db.SaveChangesAsync();

        await SetRolesAsync(link.Id, roleIds);

        TempData["Success"] = "Company link updated.";
        return RedirectToAction("Details", "Companies", new { id = link.CompanyId });
    }

    public async Task<IActionResult> Delete(long id)
    {
        var link = await db.CompanyPeople
            .AsNoTracking()
            .Include(cp => cp.Company)
            .Include(cp => cp.Person)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        return link is null ? NotFound() : View(link);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var link = await db.CompanyPeople.FindAsync(id);
        if (link is null)
            return NotFound();

        var companyId = link.CompanyId;
        db.CompanyPeople.Remove(link);
        await db.SaveChangesAsync();

        TempData["Success"] = "Person unlinked from company.";
        return RedirectToAction("Details", "Companies", new { id = companyId });
    }

    private async Task ClearPrimaryAsync(long companyId, long? exceptId = null)
    {
        var others = await db.CompanyPeople
            .Where(cp => cp.CompanyId == companyId && cp.IsPrimaryContact && cp.Id != exceptId)
            .ToListAsync();

        foreach (var cp in others)
            cp.IsPrimaryContact = false;
    }

    private async Task SetRolesAsync(long companyPersonId, long[]? roleIds)
    {
        roleIds ??= [];
        var existing = await db.CompanyPersonRoles
            .Where(r => r.CompanyPersonId == companyPersonId)
            .ToListAsync();

        db.CompanyPersonRoles.RemoveRange(existing.Where(r => !roleIds.Contains(r.CompanyRoleId)));

        var existingIds = existing.Select(r => r.CompanyRoleId).ToHashSet();
        foreach (var roleId in roleIds.Where(id => !existingIds.Contains(id)))
        {
            var roleLink = new CompanyPersonRole
            {
                CompanyPersonId = companyPersonId,
                CompanyRoleId = roleId
            };
            CrmHelpers.StampCreate(roleLink);
            db.CompanyPersonRoles.Add(roleLink);
        }

        await db.SaveChangesAsync();
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
        await PopulateRoles();
    }

    private async Task PopulateRoles()
    {
        ViewBag.Roles = await db.CompanyRoles.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
    }
}
