using System.Reflection;

namespace Minnor.Core.Metadata;

internal class EntityMapping
{
    #region Properties
    internal string TableName { get; set; }
    internal List<(PropertyInfo Property, string ColumnName)> Columns { get; set; }
    #endregion
}
