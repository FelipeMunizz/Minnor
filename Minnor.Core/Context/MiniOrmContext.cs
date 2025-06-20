﻿using Minnor.Core.Commands;

namespace Minnor.Core.Context;

public class MiniOrmContext
{
    private readonly string _connectionString;

    public MiniOrmContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Select<T> Query<T>() where T : class, new()
    {
        return new Select<T>(_connectionString);
    }

    public T Insert<T>(T entity) where T : class, new()
    {
        return new Insert<T>(_connectionString).ExecuteInsert(entity);
    }

    public T Update<T>(T entity) where T : class, new()
    {
        return new Update<T>(_connectionString).ExecuteUpdate(entity);
    }

    public bool Delete<T>(T entity) where T : class, new()
    {
        return new Delete<T>(_connectionString).ExecuteDelete(entity);
    }
}
