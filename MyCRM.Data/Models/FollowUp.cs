namespace MyCRM.Data.Models;

public class FollowUp : BaseEntity
{
    public required string Title { get; set; }

    public DateTimeOffset DueOn { get; set; }

    public bool IsDone { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
