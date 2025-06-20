using Minnor.Core.Attributes;
using System.Reflection;

namespace Minnor.Core.Metadata;

public class EntityMapper
{
    #region Properties
    private static readonly Dictionary<Type, EntityMapping> _cache = new();
    #endregion

    #region Methods
    public static EntityMapping GetMapping<T>()
    {
        var type = typeof(T);

        if (_cache.ContainsKey(type))
            return _cache[type];

        var tableName = GetTableName(type);

        var columns = type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p =>
            {                
                return (Property: p, ColumnName: GetColumnName(p));
            }).ToList();

        var mapping = new EntityMapping
        {
            TableName = tableName,
            Columns = columns
        };

        _cache[type] = mapping;
        return mapping;
    }

    private static string GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    public static string GetColumnName(PropertyInfo type)
    {
        var tableAttribute = type.GetCustomAttribute<ColumnAttribute>(); 
        return tableAttribute?.Name ?? type.Name;
    }
    #endregion
}
