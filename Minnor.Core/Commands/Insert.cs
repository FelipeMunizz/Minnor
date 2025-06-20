using Microsoft.Data.SqlClient;
using Minnor.Core.Metadata;
using Minnor.Core.Utils;
using System.Reflection;

namespace Minnor.Core.Commands;

public class Insert<T> where T : class, new()
{
    private readonly string _connectionString;

    public Insert(string connectionString)
    {
        _connectionString = connectionString;
    }

    public T CreateInsert(T entity)
    {
        try
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var type = typeof(T);

            var mapping = EntityMapper.GetMapping<T>();

            var columns = new List<string>();
            var parameters = new List<string>();
            var sqlParameters = new List<SqlParameter>();

            PropertyInfo? identityProperty = null;
            string? identityColumn = null;

            foreach (var (property, columnName) in mapping.Columns)
            {
                if (MinnorUtil.IsPrimaryKey(property, type))
                {
                    identityProperty = property;
                    identityColumn = columnName;
                    continue;
                }

                var value = property.GetValue(entity);
                if (value is null)
                    continue;

                var parameterName = $"@{columnName}";

                columns.Add(columnName);
                parameters.Add(parameterName);

                sqlParameters.Add(new SqlParameter(parameterName, value));
            }

            var sql = $"""
                       INSERT INTO [{mapping.TableName}] ({string.Join(", ", columns)})
                       VALUES ({string.Join(", ", parameters)});
                       SELECT SCOPE_IDENTITY();
                   """;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(sqlParameters.ToArray());

            var result = command.ExecuteScalar();

            if (result is not null && identityProperty is not null && identityColumn is not null)
            {
                var identityValue = Convert.ChangeType(result, identityProperty.PropertyType);
                identityProperty.SetValue(entity, identityValue);
            }

            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
