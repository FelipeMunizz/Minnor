using Microsoft.Data.SqlClient;
using System.Reflection;

namespace Minnor.Core.Models;

internal class SqlParams
{
    internal required string Sql { get; set; }
    internal required List<SqlParameter> SqlParameters { get; set; }
    internal PropertyInfo? IdentityProperty { get; set; }
    internal string? IdentityColumn { get; set; }
}
