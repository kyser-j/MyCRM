namespace MyCRM.Data.Models;

public class CompanyRole : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<CompanyPersonRole> CompanyPersonRoles { get; set; } = new List<CompanyPersonRole>();
}
