namespace MyCRM.Data.Models;

public class Email : BaseEntity
{
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    public required string EmailAddress { get; set; }

    public bool IsPrimary { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
