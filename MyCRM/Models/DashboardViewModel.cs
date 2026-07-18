using MyCRM.Data.Models;

namespace MyCRM.Models;

public class DashboardViewModel
{
    public int CompanyCount { get; set; }
    public int PeopleCount { get; set; }
    public int OpenFollowUps { get; set; }
    public int OverdueFollowUps { get; set; }
    public List<FollowUp> UpcomingFollowUps { get; set; } = [];
    public List<Company> RecentCompanies { get; set; } = [];
}
