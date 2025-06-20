using Minnor.Core.Attributes;
using System.Reflection;

namespace Minnor.Core.Utils;

public static class MinnorUtil
{
    public static string GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    public static string GetColumnName(PropertyInfo type)
    {
        var tableAttribute = type.GetCustomAttribute<ColumnAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    public static bool IsPrimaryKey(PropertyInfo property, Type type)
    {
        return property.GetCustomAttribute<KeyAttribute>() != null || 
            property.Name.ToUpper() == "ID" ||
            property.Name.ToUpper() == type.Name.ToUpper() + "ID";
    }
}
