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

    internal static bool IsCollectionProperty(PropertyInfo property)
    {
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
               && property.PropertyType != typeof(string);
    }

    internal static bool IsMappedColumn(PropertyInfo property)
    {
        var type = property.PropertyType;

        // Ignora coleções e complexos
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            return false;

        if (typeof(System.Collections.ICollection).IsAssignableFrom(type) && type != typeof(string))
            return false;

        // Ignora tipos complexos (navegação), exceto tipos básicos
        if (!type.IsValueType && type != typeof(string))
            return false;

        return true;
    }
}
