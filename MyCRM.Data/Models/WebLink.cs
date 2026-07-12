namespace MyCRM.Data.Models;

public class WebLink : BaseEntity
{
    public required string Link { get; set; }

    public required string Description { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
