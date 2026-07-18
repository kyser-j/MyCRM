using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("company_roles")]
public class CompanyRole : BaseEntity
{
    [Column("name")]
    public required string Name { get; set; }

    public ICollection<CompanyPersonRole> CompanyPersonRoles { get; set; } = new List<CompanyPersonRole>();
}
