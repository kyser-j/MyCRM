using System.ComponentModel.DataAnnotations.Schema;
using MyCRM.Data.Constants;

namespace MyCRM.Data.Models;

[Table("correspondences")]
public class Correspondence : BaseEntity
{
    [Column("subject")]
    public string? Subject { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("type")]
    public CorrespondenceType Type { get; set; }

    [Column("direction")]
    public Direction Direction { get; set; }

    [Column("occurred_at")]
    public DateTimeOffset OccurredAt { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
