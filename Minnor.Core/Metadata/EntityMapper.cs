using Minnor.Core.Utils;

namespace Minnor.Core.Metadata;

public class EntityMapper
{
    #region Methods
    internal static EntityMapping GetMapping<T>()
    {
        var type = typeof(T);

        return GetMappingInternal(type);
    }

    internal static EntityMapping GetMapping(Type type) =>
        GetMappingInternal(type, isMappadColumn: true);

    private static EntityMapping GetMappingInternal(Type type, bool isMappadColumn = false)
    {
        var tableName = MinnorUtil.GetTableName(type);

        var columns = type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p =>
            {
                return (Property: p, ColumnName: MinnorUtil.GetColumnName(p));
            }).ToList();

        if (isMappadColumn)
            columns = type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite && MinnorUtil.IsMappedColumn(p))
            .Select(p =>
            {
                return (Property: p, ColumnName: MinnorUtil.GetColumnName(p));
            }).ToList();

        var mapping = new EntityMapping
        {
            TableName = tableName,
            Columns = columns
        };

        return mapping;
    }
    #endregion
}
