using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("phones")]
public class Phone : BaseEntity
{
    [Column("phone_number")]
    public required string PhoneNumber { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
