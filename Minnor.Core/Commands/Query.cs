using Microsoft.Data.SqlClient;
using Minnor.Core.Metadata;
using Minnor.Core.Sql;
using System.Linq.Expressions;

namespace Minnor.Core.Commands;

public class Query<T> where T : class, new()
{
    #region Properties
    private readonly string _connectionString;
    private Expression<Func<T, bool>>? _whereExpression;
    private Func<T, object>? _orderBy;
    private bool _descending;
    #endregion

    #region Constructors
    public Query(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    #endregion

    #region Methods
    public Query<T> Where(Expression<Func<T, bool>> expression)
    {
        _whereExpression = expression;
        return this;
    }

    public Query<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool descending = false)
    {
        if(keySelector is null)
            throw new ArgumentNullException(nameof(keySelector));

        _orderBy = x => keySelector.Compile()(x);
        _descending = descending;
        return this;
    }

    public List<T> ToList()
    {
        var mapping = EntityMapper.GetMapping<T>();
        var columns = string.Join(",", mapping.Columns.Select(c => c.ColumnName));
        var table = mapping.TableName;

        var whereSql = _whereExpression is not null ?
            new SqlExpressionVisitor().Translate(_whereExpression) :
            string.Empty;

        var orderSql = _orderBy is not null ?
            $" ORDER BY {GetOrderColumnName(mapping)} {(_descending ? "DESC" : "ASC")}" :
            string.Empty;

        var sql = $"SELECT {columns} FROM [{table}] {whereSql} {orderSql}".Trim();

        return ExecuteSelect<T>(sql);
    }

    private string GetOrderColumnName(EntityMapping mapping)
    {
        var body = (_orderBy?.Method.GetParameters().FirstOrDefault()?.Name ?? "").ToLower();
        return mapping.Columns.First().ColumnName;
    }

    private List<T> ExecuteSelect<T>(string command) where T : class, new ()
    {
        var mapping = EntityMapper.GetMapping<T>();
        var result = new List<T>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand(command, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var entity = new T();

            for(int i = 0;  i < mapping.Columns.Count; i++)
            {
                var value = reader.GetValue(i);
                mapping.Columns[i].Property.SetValue(entity, value == DBNull.Value? null : value);
            }

            result.Add(entity);
        }

        return result;
    }
    #endregion
}
