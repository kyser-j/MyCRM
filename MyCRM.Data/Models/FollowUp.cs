using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("follow_ups")]
public class FollowUp : BaseEntity
{
    [Column("title")]
    public required string Title { get; set; }

    [Column("due_on")]
    public DateTimeOffset DueOn { get; set; }

    [Column("is_done")]
    public bool IsDone { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
