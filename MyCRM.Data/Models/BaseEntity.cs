namespace MyCRM.Data.Models;

public abstract class BaseEntity : IEntity
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
