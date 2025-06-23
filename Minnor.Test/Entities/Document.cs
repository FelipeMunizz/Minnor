using Minnor.Core.Attributes;

namespace Minnor.Test.Entities;

public class Document
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User User { get; set; }
}
