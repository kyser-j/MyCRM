using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("company_people")]
public class CompanyPerson : BaseEntity
{
    [Column("company_id")]
    public long CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    [Column("person_id")]
    public long PersonId { get; set; }

    public Person Person { get; set; } = null!;

    [Column("is_primary_contact")]
    public bool IsPrimaryContact { get; set; }

    public ICollection<CompanyPersonRole> CompanyPersonRoles { get; set; } = new List<CompanyPersonRole>();
}
