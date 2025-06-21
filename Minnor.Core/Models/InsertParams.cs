using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Minnor.Core.Models;

public class InsertParams
{
    public required string Sql { get; set; }
    public required List<SqlParameter> SqlParameters { get; set; }
    public PropertyInfo? IdentityProperty { get; set; }
    public string? IdentityColumn { get; set; }
}
