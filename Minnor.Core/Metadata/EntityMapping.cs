using System.Reflection;

namespace Minnor.Core.Metadata;

public class EntityMapping
{
    #region Properties
    public string TableName { get; set; }
    public List<(PropertyInfo Property, string ColumnName)> Columns { get; set; }
    #endregion
}
