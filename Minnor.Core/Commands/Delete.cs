using Microsoft.Data.SqlClient;
using Minnor.Core.Metadata;
using Minnor.Core.Models;
using Minnor.Core.Utils;
using System.Reflection;

namespace Minnor.Core.Commands;

internal class Delete<T> where T : class, new()
{
    private readonly string _connectionString;
    internal Delete(string connectionString)
    {
        _connectionString = connectionString;
    }

    internal bool ExecuteDelete(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var deleteParams = CreateDelete(entity);

        return ExecuteNonQuery(deleteParams.Sql, deleteParams.SqlParameters.FirstOrDefault()!) > 0;
    }

    private SqlParams CreateDelete(T entity)
    {
        var type = typeof(T);

        var mapping = EntityMapper.GetMapping<T>();
        PropertyInfo identityProperty = mapping.Columns.FirstOrDefault(p => MinnorUtil.IsPrimaryKey(p.Property, type)).Property;

        int value = Convert.ChangeType(identityProperty.GetValue(entity), identityProperty.PropertyType) as int? ?? 0;

        if (value == 0)
            throw new InvalidOperationException("Entity does not have a valid primary key value.");

        SqlParameter identityParameter = new SqlParameter($"@{identityProperty.Name}", value);

        string sql = $"DELETE FROM [{mapping.TableName}] WHERE [{identityProperty.Name}] = @{identityProperty.Name}";

        return new SqlParams
        {
            Sql = sql,
            SqlParameters = new List<SqlParameter> { identityParameter }
        };
    }

    private int ExecuteNonQuery(string sql, SqlParameter identityParameter)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(identityParameter);

            return command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            // Handle SQL exceptions, log them or rethrow as needed
            throw new InvalidOperationException("An error occurred while executing the delete operation.", ex);
        }
    }
}
