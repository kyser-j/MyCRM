using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("company_person_roles")]
public class CompanyPersonRole : BaseEntity
{
    [Column("company_person_id")]
    public long CompanyPersonId { get; set; }

    public CompanyPerson CompanyPerson { get; set; } = null!;

    [Column("company_role_id")]
    public long CompanyRoleId { get; set; }

    public CompanyRole CompanyRole { get; set; } = null!;
}
