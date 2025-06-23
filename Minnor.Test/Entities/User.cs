using Minnor.Core.Attributes;

namespace Minnor.Test.Entities;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Senha { get; set; }

    public string Nome { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
