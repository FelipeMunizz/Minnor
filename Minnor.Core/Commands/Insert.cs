using Microsoft.Data.SqlClient;
using Minnor.Core.Metadata;
using Minnor.Core.Models;
using Minnor.Core.Utils;
using System.Reflection;

namespace Minnor.Core.Commands;

internal class Insert<T> where T : class, new()
{
    #region Properties
    private readonly string _connectionString;
    #endregion

    #region Constructors
    internal Insert(string connectionString)
    {
        _connectionString = connectionString;
    }
    #endregion

    #region Methods
    internal T ExecuteInsert(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var insertParams = CreateInsert(entity);

        var result = ExecuteScalar(insertParams.Sql, insertParams.SqlParameters);

        if (result is not null && insertParams.IdentityProperty is not null && insertParams.IdentityColumn is not null)
        {
            var identityValue = Convert.ChangeType(result, insertParams.IdentityProperty.PropertyType);
            insertParams.IdentityProperty.SetValue(entity, identityValue);
        }

        return entity;
    }

    private SqlParams CreateInsert(T entity)
    {
        var type = typeof(T);

        var mapping = EntityMapper.GetMapping(type);

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

        return new SqlParams
        {
            Sql = sql,
            SqlParameters = sqlParameters,
            IdentityProperty = identityProperty,
            IdentityColumn = identityColumn
        };
    }

    private object ExecuteScalar(string sql, List<SqlParameter> sqlParameters)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(sqlParameters.ToArray());

            return command.ExecuteScalar() ?? throw new InvalidOperationException("Insert operation did not return an identity value.");
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException("An error occurred while executing the insert command.", ex);
        }
    }
    #endregion
}
