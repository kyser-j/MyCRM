using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class CompanyRolesController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var roles = await db.CompanyRoles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync();

        return View(roles);
    }

    public IActionResult Create() => View(new CompanyRole { Name = string.Empty });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyRole model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        else if (await db.CompanyRoles.AnyAsync(r => r.Name == model.Name.Trim()))
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");

        if (!ModelState.IsValid)
            return View(model);

        var role = new CompanyRole { Name = model.Name.Trim() };
        CrmHelpers.StampCreate(role);

        db.CompanyRoles.Add(role);
        await db.SaveChangesAsync();

        TempData["Success"] = "Role created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var role = await db.CompanyRoles.FindAsync(id);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, CompanyRole model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        else if (await db.CompanyRoles.AnyAsync(r => r.Name == model.Name.Trim() && r.Id != id))
            ModelState.AddModelError(nameof(model.Name), "A role with this name already exists.");

        if (!ModelState.IsValid)
            return View(model);

        var role = await db.CompanyRoles.FindAsync(id);
        if (role is null)
            return NotFound();

        role.Name = model.Name.Trim();
        CrmHelpers.StampUpdate(role);
        await db.SaveChangesAsync();

        TempData["Success"] = "Role updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(long id)
    {
        var role = await db.CompanyRoles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return role is null ? NotFound() : View(role);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var role = await db.CompanyRoles.FindAsync(id);
        if (role is null)
            return NotFound();

        db.CompanyRoles.Remove(role);
        await db.SaveChangesAsync();

        TempData["Success"] = "Role deleted.";
        return RedirectToAction(nameof(Index));
    }
}
