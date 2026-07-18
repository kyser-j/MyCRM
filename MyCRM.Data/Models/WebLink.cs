using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

[Table("web_links")]
public class WebLink : BaseEntity
{
    [Column("link")]
    public required string Link { get; set; }

    [Column("description")]
    public required string Description { get; set; }

    [Column("person_id")]
    public long? PersonId { get; set; }

    public Person? Person { get; set; }

    [Column("company_id")]
    public long? CompanyId { get; set; }

    public Company? Company { get; set; }
}
