namespace MyCRM.Data.Models;

public class CompanyPersonRole : BaseEntity
{
    public long CompanyPersonId { get; set; }

    public CompanyPerson CompanyPerson { get; set; } = null!;

    public long CompanyRoleId { get; set; }

    public CompanyRole CompanyRole { get; set; } = null!;
}
