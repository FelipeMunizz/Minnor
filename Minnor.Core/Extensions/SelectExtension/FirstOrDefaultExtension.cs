using Microsoft.Data.SqlClient;
using Minnor.Core.Commands;
using Minnor.Core.Metadata;
using Minnor.Core.Sql;
using Minnor.Core.Utils;
using System.Linq.Expressions;

namespace Minnor.Core.Extensions.SelectExtension;

public static class FirstOrDefaultExtension
{

    public static T? FirstOrDefault<T>(this Select<T> select, Expression<Func<T, bool>>? predicate = null) where T : class, new()
        => CreateFirstOrDefault(select, predicate);

    private static T? CreateFirstOrDefault<T>(Select<T> select, Expression<Func<T, bool>>? predicate = null) where T : class, new()
    {
        var type = typeof(T);
        var mapping = EntityMapper.GetMapping(type);
        var columns = string.Join(",", mapping.Columns
            .Where(c => !MinnorUtil.IsCollectionProperty(c.Property))
            .Select(c => c.ColumnName));

        var table = mapping.TableName;

        var whereSql = predicate is not null ?
            new SqlExpressionVisitor().Translate(predicate) :
            string.Empty;

        var sql = $"SELECT TOP 1 {columns} FROM [{table}] {whereSql}";

        return ExecuteFirstOrDefault(select, sql, mapping);
    }

    private static T? ExecuteFirstOrDefault<T>(this Select<T> select, string sql, EntityMapping mapping) where T : class, new()
    { 
        var connection = ValidateConnection(select);

        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();
        
        var selectedColumns = mapping.Columns.Where(c => !MinnorUtil.IsCollectionProperty(c.Property)).ToList();
        
        var entity = new T();

        while (reader.Read())
        {
            for (int i = 0; i < selectedColumns.Count; i++)
            {
                var value = reader.GetValue(i);
                mapping.Columns[i].Property.SetValue(entity, value == DBNull.Value ? null : value);
            }
        }

        return entity;
    }

    private static SqlConnection ValidateConnection<T>(Select<T> select) where T : class, new()
    {
        var connectionField = typeof(Select<T>).GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (connectionField is null)
            throw new InvalidOperationException("Connection field not found in Select<T>.");

        var connection = connectionField.GetValue(select) as SqlConnection;

        if (connection is null)
            throw new InvalidOperationException("Connection is null.");

        return connection;
    }
}
