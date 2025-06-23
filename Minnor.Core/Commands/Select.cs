using Microsoft.Data.SqlClient;
using Minnor.Core.Attributes;
using Minnor.Core.Metadata;
using Minnor.Core.Sql;
using Minnor.Core.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace Minnor.Core.Commands;

public class Select<T> where T : class, new()
{
    #region Properties
    private readonly string _connectionString;
    private Expression<Func<T, bool>>? _whereExpression;
    private Func<T, object>? _orderBy;
    private bool _descending;
    private List<string> _includes = new();
    #endregion

    #region Constructors
    public Select(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    #endregion

    #region Methods
    public Select<T> Where(Expression<Func<T, bool>> expression)
    {
        _whereExpression = expression;
        return this;
    }

    public Select<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool descending = false)
    {
        if (keySelector is null)
            throw new ArgumentNullException(nameof(keySelector));

        _orderBy = x => keySelector.Compile()(x);
        _descending = descending;
        return this;
    }

    public Select<T> Include<TProperty>(Expression<Func<T, TProperty>> navigationProperty)
    {
        if (navigationProperty.Body is MemberExpression memberExpression)
            _includes.Add(memberExpression.Member.Name);

        return this;
    }

    public List<T> ToList()
    {
        var mapping = EntityMapper.GetMapping<T>();
        var columns = string.Join(",", mapping.Columns
            .Where(c => !MinnorUtil.IsCollectionProperty(c.Property))
            .Select(c => c.ColumnName));

        var table = mapping.TableName;

        var whereSql = _whereExpression is not null ?
            new SqlExpressionVisitor().Translate(_whereExpression) :
            string.Empty;

        var orderSql = _orderBy is not null ?
            $" ORDER BY {GetOrderColumnName(mapping)} {(_descending ? "DESC" : "ASC")}" :
            string.Empty;

        var sql = $"SELECT {columns} FROM [{table}] {whereSql} {orderSql}".Trim();

        var list = ExecuteSelect<T>(sql);

        if (_includes.Any())
            LoadIncludes(list);

        return list;
    }

    private void LoadIncludes(List<T> entities)
    {
        foreach (var include in _includes)
        {
            var navProperty = typeof(T).GetProperty(include);

            if (navProperty is null)
                continue;

            var isCollection = typeof(System.Collections.IEnumerable).IsAssignableFrom(navProperty.PropertyType)
                && navProperty.PropertyType != typeof(string);

            if (isCollection)
                LoadCollectionInclude(entities, navProperty);
            else
                LoadReferenceInclude(entities, navProperty);
        }
    }

    private void LoadReferenceInclude(List<T> entites, PropertyInfo property)
    {
        var foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();

        if (foreignKey is null) return;

        var foreignKeyProperty = typeof(T).GetProperty(foreignKey.NavigationPropertyName);
        if (foreignKeyProperty is null) return;

        var navType = foreignKeyProperty.PropertyType;
        var navMapping = EntityMapper.GetMapping<T>();
        var navKey = navMapping.Columns.FirstOrDefault(c => c.Property == foreignKeyProperty);

        foreach (var iten in entites)
        {
            var fkValue = foreignKeyProperty.GetValue(iten);
            if (fkValue is null) continue;

            var command = $@"
                    SELECT {string.Join(",", navMapping.Columns.Select(c => c.ColumnName))} 
                        FROM [{navMapping.TableName}] 
                    WHERE [{navKey.ColumnName}] = @ForeignKeyValue
            ";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(command, conn);
            cmd.Parameters.AddWithValue("@ForeignKeyValue", fkValue);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var related = Activator.CreateInstance(navType);

                for (int i = 0; i < navMapping.Columns.Count; i++)
                {
                    var value = reader.GetValue(i);
                    navMapping.Columns[i].Property.SetValue(related, value == DBNull.Value ? null : value);
                }

                property.SetValue(iten, related);
            }
        }
    }

    private void LoadCollectionInclude(List<T> entities, PropertyInfo property)
    {
        var elementType = property.PropertyType.GenericTypeArguments.FirstOrDefault();
        if (elementType == null) return;

        var navMapping = EntityMapper.GetMapping(elementType);

        var fkInChild = elementType.GetProperties()

        .Select(p => new
        {
            Property = p,
            Attr = p.GetCustomAttributes(typeof(ForeignKeyAttribute), false)
                    .FirstOrDefault() as ForeignKeyAttribute
        })
        .FirstOrDefault(p => p.Attr?.NavigationPropertyName == typeof(T).Name);

        if (fkInChild == null) return;

        var fkProp = fkInChild.Property;
        var fkColumn = MinnorUtil.GetColumnName(fkProp);

        // Obtem todos os IDs do pai (ex: Person.Id)
        var parentIdProp = typeof(T).GetProperty("Id");
        if (parentIdProp == null) return;

        var parentIdToEntity = entities.ToDictionary(
            parent => parentIdProp.GetValue(parent),
            parent => parent
        );

        var parentIds = parentIdToEntity.Keys.Where(id => id != null).Distinct().ToList();

        if (!parentIds.Any()) return;

        // Monta lista de parâmetros
        var paramNames = parentIds.Select((id, index) => $"@p{index}").ToList();
        var sql = $"SELECT * FROM [{navMapping.TableName}] WHERE [{fkColumn}] IN ({string.Join(",", paramNames)})";

        var children = new List<object>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);

        for (int i = 0; i < parentIds.Count; i++)
            cmd.Parameters.AddWithValue(paramNames[i], parentIds[i]);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var child = Activator.CreateInstance(elementType);
            for (int i = 0; i < navMapping.Columns.Count; i++)
            {
                var value = reader.GetValue(i);
                navMapping.Columns[i].Property.SetValue(child, value == DBNull.Value ? null : value);
            }
            children.Add(child!);
        }

        // Agrupa os filhos por valor da chave estrangeira
        var childrenGrouped = children
            .GroupBy(child => fkProp.GetValue(child))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Atribui para os respectivos pais
        foreach (var parentId in parentIds)
        {
            if (parentIdToEntity.TryGetValue(parentId, out var parent))
            {
                var collection = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
                if (childrenGrouped.TryGetValue(parentId, out var items))
                {
                    foreach (var child in items)
                        collection.Add(child!);
                }
                property.SetValue(parent, collection);
            }
        }

    }

    private string GetOrderColumnName(EntityMapping mapping)
    {
        var body = (_orderBy?.Method.GetParameters().FirstOrDefault()?.Name ?? "").ToLower();
        return mapping.Columns.First().ColumnName;
    }

    private List<T> ExecuteSelect<T>(string command) where T : class, new()
    {
        var mapping = EntityMapper.GetMapping<T>();
        var result = new List<T>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand(command, conn);
        using var reader = cmd.ExecuteReader();

        var selectedColumns = mapping.Columns.Where(c => !MinnorUtil.IsCollectionProperty(c.Property)).ToList();

        while (reader.Read())
        {
            var entity = new T();

            for (int i = 0; i < selectedColumns.Count; i++)
            {
                var value = reader.GetValue(i);
                mapping.Columns[i].Property.SetValue(entity, value == DBNull.Value ? null : value);
            }

            result.Add(entity);
        }

        return result;
    }
    #endregion
}
