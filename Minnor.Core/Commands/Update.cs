using Microsoft.Data.SqlClient;
using Minnor.Core.Metadata;
using Minnor.Core.Models;
using Minnor.Core.Utils;
using System.Reflection;

namespace Minnor.Core.Commands;

internal class Update<T> where T : class, new()
{
	#region Properties
	private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;
    #endregion

    #region Constructors
    internal Update(SqlConnection connection, SqlTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }
    #endregion

    #region Methods
    internal T ExecuteUpdate(T entity)
    {
        if(entity is null)
            throw new ArgumentNullException(nameof(entity));

        var updateParams = CreateUpdate(entity);

        var rowsAffected = ExecuteNonQuery(updateParams.Sql, updateParams.SqlParameters);

        if (rowsAffected == 0)
            throw new InvalidOperationException("Update operation did not affect any rows. Entity may not exist in the database.");

        return entity;
    }

    private SqlParams CreateUpdate(T entity)
    {
        var type = typeof(T);

        var mapping = EntityMapper.GetMapping(type);

        var columnsAndParameters = new Dictionary<string, string>();
        var sqlParameters = new List<SqlParameter>();

        PropertyInfo? identityProperty = null;
        string? identityColumn = null;
        SqlParameter identityParameter = new SqlParameter();

        foreach (var (property, columnName) in mapping.Columns)
        {
            var value = property.GetValue(entity);

            if (MinnorUtil.IsPrimaryKey(property, type))
            {
                identityProperty = property;
                identityColumn = columnName;

                if (value is null)
                    throw new InvalidOperationException($"Primary key property '{property.Name}' cannot be null for update operation.");
                
                identityParameter = new SqlParameter($"@{identityColumn}", value);
                continue;
            }

            if (value is null)
                continue;

            columnsAndParameters.Add(columnName, $"@{columnName}");
            sqlParameters.Add(new SqlParameter($"@{columnName}", value));
        }

        var sql = $"UPDATE [{mapping.TableName}] SET " +
            string.Join(", ", columnsAndParameters.Select(kvp => $"[{kvp.Key}] = {kvp.Value}")) +
            $" WHERE [{identityColumn}] = @{identityColumn}";

        sqlParameters.Add(identityParameter);

        return new SqlParams
        {
            Sql = sql,
            SqlParameters = sqlParameters,
            IdentityProperty = identityProperty,
            IdentityColumn = identityColumn
        };
    }

    private int ExecuteNonQuery(string sql, List<SqlParameter> sqlParameters)
    {
        try
        {
            using var command = new SqlCommand(sql, _connection, _transaction);
            command.Parameters.AddRange(sqlParameters.ToArray());

            return command.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException("An error occurred while executing the update operation.", ex);
        }
    }
    #endregion
}
