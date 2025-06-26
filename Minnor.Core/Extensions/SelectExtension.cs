using Microsoft.Data.SqlClient;
using Minnor.Core.Commands;
using Minnor.Core.Metadata;
using System.Reflection;

namespace Minnor.Core.Extensions;

public static class SelectExtension
{
    public static List<T> CustomQuery<T>(this Select<T> _, string query) where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentNullException(nameof(query));

        var connectionStringField = typeof(Select<T>)
            .GetField("_connection", BindingFlags.NonPublic | BindingFlags.Instance);

        if (connectionStringField is null)
            throw new InvalidOperationException("Connection string field not found in Select<T>.");

        var connection = connectionStringField.GetValue(_) as SqlConnection;

        return ExecuteCustomQuery<T>(query, connection ?? 
            throw new InvalidOperationException("Connection string is null."));
    }

    private static List<T> ExecuteCustomQuery<T>(string sql, SqlConnection connection) where T : class, new()
    {
        var result = new List<T>();
        var mapping = EntityMapper.GetMapping<T>();
        var columnToPropertyMap = mapping.Columns.ToDictionary(c => c.ColumnName.ToLower(), c => c.Property);

        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        var columnNames = Enumerable.Range(0, reader.FieldCount)
        .Select(reader.GetName)
        .ToList();

        while (reader.Read())
        {
            var entity = new T();

            foreach (var colName in columnNames)
            {
                var key = colName.ToLower();

                if (columnToPropertyMap.TryGetValue(key, out var property))
                {
                    var value = reader[colName];
                    if (value != DBNull.Value)
                    {
                        property.SetValue(entity, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }

            result.Add(entity);
        }

        return result;
    }
}
