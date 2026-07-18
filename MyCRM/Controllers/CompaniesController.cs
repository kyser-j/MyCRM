using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Constants;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class CompaniesController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? q, LeadStatus? status)
    {
        var query = db.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                (c.City != null && c.City.Contains(term)) ||
                (c.Industry != null && c.Industry.Value.ToString().Contains(term)));
        }

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        ViewBag.StatusFilter = CrmHelpers.NullableEnumSelectList(status, "All statuses");
        ViewBag.Search = q;

        var companies = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(companies);
    }

    public async Task<IActionResult> Details(long id)
    {
        var company = await db.Companies
            .AsNoTracking()
            .Include(c => c.CompanyPeople).ThenInclude(cp => cp.Person)
            .Include(c => c.CompanyPeople).ThenInclude(cp => cp.CompanyPersonRoles).ThenInclude(r => r.CompanyRole)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.WebLinks)
            .Include(c => c.Notes)
            .Include(c => c.FollowUps)
            .Include(c => c.CorrespondenceRecords)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (company is null)
            return NotFound();

        return View(company);
    }

    public IActionResult Create()
    {
        PopulateLookups();
        return View(new Company { Name = string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Company model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");

        if (!ModelState.IsValid)
        {
            PopulateLookups(model);
            return View(model);
        }

        var company = new Company
        {
            Name = model.Name.Trim(),
            Industry = model.Industry,
            Street = NullIfEmpty(model.Street),
            City = NullIfEmpty(model.City),
            State = NullIfEmpty(model.State),
            Country = NullIfEmpty(model.Country),
            PostalCode = NullIfEmpty(model.PostalCode),
            EmployeeCount = model.EmployeeCount,
            Status = model.Status,
            Source = model.Source
        };
        CrmHelpers.StampCreate(company);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        TempData["Success"] = "Company created.";
        return RedirectToAction(nameof(Details), new { id = company.Id });
    }

    public async Task<IActionResult> Edit(long id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company is null)
            return NotFound();

        PopulateLookups(company);
        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Company model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");

        if (!ModelState.IsValid)
        {
            PopulateLookups(model);
            return View(model);
        }

        var company = await db.Companies.FindAsync(id);
        if (company is null)
            return NotFound();

        company.Name = model.Name.Trim();
        company.Industry = model.Industry;
        company.Street = NullIfEmpty(model.Street);
        company.City = NullIfEmpty(model.City);
        company.State = NullIfEmpty(model.State);
        company.Country = NullIfEmpty(model.Country);
        company.PostalCode = NullIfEmpty(model.PostalCode);
        company.EmployeeCount = model.EmployeeCount;
        company.Status = model.Status;
        company.Source = model.Source;
        CrmHelpers.StampUpdate(company);

        await db.SaveChangesAsync();
        TempData["Success"] = "Company updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(long id)
    {
        var company = await db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (company is null)
            return NotFound();

        return View(company);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company is null)
            return NotFound();

        db.Companies.Remove(company);
        await db.SaveChangesAsync();

        TempData["Success"] = "Company deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void PopulateLookups(Company? company = null)
    {
        ViewBag.Industries = CrmHelpers.NullableEnumSelectList(company?.Industry);
        ViewBag.Statuses = CrmHelpers.EnumSelectList<LeadStatus>(company?.Status ?? LeadStatus.Lead);
        ViewBag.Sources = CrmHelpers.EnumSelectList<LeadSource>(company?.Source ?? LeadSource.Unknown);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
