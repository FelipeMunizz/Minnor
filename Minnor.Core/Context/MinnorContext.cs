using Microsoft.Data.SqlClient;
using Minnor.Core.Commands;

namespace Minnor.Core.Context;

public class MinnorContext : IDisposable
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;

    public MinnorContext(string connectionString)
    {
        _connection= new SqlConnection(connectionString);
        _connection.Open();
    }

    public void BeginTransaction()
    {
        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction = null;
    }

    public Select<T> Query<T>() where T : class, new()
        => new Select<T>(_connection);


    public T Insert<T>(T entity) where T : class, new() =>
        new Insert<T>(_connection, _transaction).ExecuteInsert(entity);


    public T Update<T>(T entity) where T : class, new() =>
        new Update<T>(_connection, _transaction).ExecuteUpdate(entity);


    public bool Delete<T>(T entity) where T : class, new() =>
        new Delete<T>(_connection, _transaction).ExecuteDelete(entity);

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection.Dispose();
    }
}
