namespace MyCRM.Data.Models;

public class Note : BaseEntity
{
    public required string Content { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
