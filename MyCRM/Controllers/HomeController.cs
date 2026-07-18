using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCRM.Data;
using MyCRM.Models;
using System.Diagnostics;

namespace MyCRM.Controllers;

public class HomeController(MyDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var now = DateTimeOffset.UtcNow;

        var vm = new DashboardViewModel
        {
            CompanyCount = await db.Companies.CountAsync(),
            PeopleCount = await db.People.CountAsync(),
            OpenFollowUps = await db.FollowUps.CountAsync(f => !f.IsDone),
            OverdueFollowUps = await db.FollowUps.CountAsync(f => !f.IsDone && f.DueOn < now),
            UpcomingFollowUps = await db.FollowUps
                .AsNoTracking()
                .Include(f => f.Company)
                .Include(f => f.Person)
                .Where(f => !f.IsDone)
                .OrderBy(f => f.DueOn)
                .Take(8)
                .ToListAsync(),
            RecentCompanies = await db.Companies
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
