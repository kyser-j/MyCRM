using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("notes")]
public class Note : BaseEntity
{
    [Column("content")]
    public required string Content { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
