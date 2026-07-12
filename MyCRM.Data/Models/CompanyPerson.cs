namespace MyCRM.Data.Models;

public class CompanyPerson : BaseEntity
{
    public long CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public long PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public bool IsPrimaryContact { get; set; }

    public ICollection<CompanyPersonRole> CompanyPersonRoles { get; set; } = new List<CompanyPersonRole>();
}
