namespace MyCRM.Data.Models;

public class Phone : BaseEntity
{
    public required string PhoneNumber { get; set; }

    public long? CompanyId { get; set; }

    public Company? Company { get; set; }

    public bool IsPrimary { get; set; }

    public long? PersonId { get; set; }

    public Person? Person { get; set; }
}
