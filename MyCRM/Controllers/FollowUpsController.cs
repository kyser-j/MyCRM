using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Data.Models;
using MyCRM.Helpers;

namespace MyCRM.Controllers;

public class FollowUpsController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index(bool? done)
    {
        var query = db.FollowUps
            .AsNoTracking()
            .Include(f => f.Company)
            .Include(f => f.Person)
            .AsQueryable();

        if (done.HasValue)
            query = query.Where(f => f.IsDone == done.Value);

        ViewBag.DoneFilter = done;

        var items = await query
            .OrderBy(f => f.IsDone)
            .ThenBy(f => f.DueOn)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Create(long? companyId, long? personId)
    {
        await PopulateLookups(companyId, personId);
        return View(new FollowUp
        {
            Title = string.Empty,
            DueOn = DateTimeOffset.Now.Date.AddDays(1).AddHours(9),
            CompanyId = companyId,
            PersonId = personId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FollowUp model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Title is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var followUp = new FollowUp
        {
            Title = model.Title.Trim(),
            DueOn = model.DueOn,
            IsDone = model.IsDone,
            CompletedAt = model.IsDone ? DateTimeOffset.UtcNow : null,
            CompanyId = model.CompanyId,
            PersonId = model.PersonId
        };
        CrmHelpers.StampCreate(followUp);

        db.FollowUps.Add(followUp);
        await db.SaveChangesAsync();

        TempData["Success"] = "Follow-up created.";
        return RedirectToParentOrIndex(followUp);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var followUp = await db.FollowUps.FindAsync(id);
        if (followUp is null)
            return NotFound();

        await PopulateLookups(followUp.CompanyId, followUp.PersonId);
        return View(followUp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, FollowUp model)
    {
        if (id != model.Id)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Title is required.");

        if (!ModelState.IsValid)
        {
            await PopulateLookups(model.CompanyId, model.PersonId);
            return View(model);
        }

        var followUp = await db.FollowUps.FindAsync(id);
        if (followUp is null)
            return NotFound();

        var wasDone = followUp.IsDone;
        followUp.Title = model.Title.Trim();
        followUp.DueOn = model.DueOn;
        followUp.IsDone = model.IsDone;
        followUp.CompanyId = model.CompanyId;
        followUp.PersonId = model.PersonId;

        if (model.IsDone && !wasDone)
            followUp.CompletedAt = DateTimeOffset.UtcNow;
        else if (!model.IsDone)
            followUp.CompletedAt = null;

        CrmHelpers.StampUpdate(followUp);
        await db.SaveChangesAsync();

        TempData["Success"] = "Follow-up updated.";
        return RedirectToParentOrIndex(followUp);
    }

    public async Task<IActionResult> Delete(long id)
    {
        var followUp = await db.FollowUps
            .AsNoTracking()
            .Include(f => f.Company)
            .Include(f => f.Person)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (followUp is null)
            return NotFound();

        return View(followUp);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var followUp = await db.FollowUps.FindAsync(id);
        if (followUp is null)
            return NotFound();

        db.FollowUps.Remove(followUp);
        await db.SaveChangesAsync();

        TempData["Success"] = "Follow-up deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDone(long id)
    {
        var followUp = await db.FollowUps.FindAsync(id);
        if (followUp is null)
            return NotFound();

        followUp.IsDone = !followUp.IsDone;
        followUp.CompletedAt = followUp.IsDone ? DateTimeOffset.UtcNow : null;
        CrmHelpers.StampUpdate(followUp);
        await db.SaveChangesAsync();

        TempData["Success"] = followUp.IsDone ? "Marked as done." : "Marked as open.";
        return Redirect(Request.Headers.Referer.ToString() is { Length: > 0 } referer
            ? referer
            : Url.Action(nameof(Index))!);
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

    private IActionResult RedirectToParentOrIndex(FollowUp followUp)
    {
        if (followUp.CompanyId.HasValue)
            return RedirectToAction("Details", "Companies", new { id = followUp.CompanyId });
        if (followUp.PersonId.HasValue)
            return RedirectToAction("Details", "People", new { id = followUp.PersonId });
        return RedirectToAction(nameof(Index));
    }
}
