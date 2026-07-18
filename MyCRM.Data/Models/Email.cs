using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("emails")]
public class Email : BaseEntity
{
    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Column("email_address")]
    public required string EmailAddress { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
