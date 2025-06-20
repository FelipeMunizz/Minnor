using Minnor.Core.Commands;

namespace Minnor.Core.Context;

public class MiniOrmContext
{
    private readonly string _connectionString;

    public MiniOrmContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Query<T> Query<T>() where T : class, new()
    {
        return new Query<T>(_connectionString);
    }

    public T Insert<T>(T entity) where T : class, new()
    {
        return new Insert<T>(_connectionString).CreateInsert(entity);
    }
}
