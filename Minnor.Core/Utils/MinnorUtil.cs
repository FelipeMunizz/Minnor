using Minnor.Core.Attributes;
using System.Reflection;

namespace Minnor.Core.Utils;

internal static class MinnorUtil
{
    internal static string GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    internal static string GetColumnName(PropertyInfo type)
    {
        var tableAttribute = type.GetCustomAttribute<ColumnAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    internal static bool IsPrimaryKey(PropertyInfo property, Type type)
    {
        return property.GetCustomAttribute<KeyAttribute>() != null || 
            property.Name.ToUpper() == "ID" ||
            property.Name.ToUpper() == type.Name.ToUpper() + "ID";
    }
}
