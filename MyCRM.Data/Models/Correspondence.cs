using MyCRM.Data.Constants;

namespace MyCRM.Data.Models;

public class Correspondence : BaseEntity
{
    public string? Subject { get; set; }

    public string? Content { get; set; }

    public CorrespondenceType Type { get; set; }

    public Direction Direction { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
