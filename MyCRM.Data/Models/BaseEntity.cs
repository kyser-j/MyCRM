using System.ComponentModel.DataAnnotations.Schema;

namespace MyCRM.Data.Models;

public abstract class BaseEntity : IEntity
{
    [Column("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
