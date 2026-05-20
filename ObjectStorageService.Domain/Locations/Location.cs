using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Service.Template.Locations;

[Table(nameof(Location))]
public class Location : AuditedAggregateRoot<Guid>
{
    public string Title { get; set; }
    public string Code { get; set; }
    public Guid? ParentId { get; set; }
    public virtual Location? Parent { get; set; }
    public virtual ICollection<Location>? Children { get; set; }
}
