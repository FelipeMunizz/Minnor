using Minnor.Core.Attributes;
using Minnor.Core.Utils;
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

        var tableName = MinnorUtil.GetTableName(type);

        var columns = type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p =>
            {                
                return (Property: p, ColumnName: MinnorUtil.GetColumnName(p));
            }).ToList();

        var mapping = new EntityMapping
        {
            TableName = tableName,
            Columns = columns
        };

        _cache[type] = mapping;
        return mapping;
    }
    #endregion
}
