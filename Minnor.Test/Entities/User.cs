using Minnor.Core.Attributes;

namespace Minnor.Test.Entities;

[Table("TB_USER")]
public class User
{
    [Column("ID")]
    public long Id { get; set; }

    [Column("EMAIL")]
    public string Email { get; set; }

    [Column("PASSWORD")]
    public string Senha { get; set; }

    [Column("NAME")]
    public string Nome { get; set; }
}
