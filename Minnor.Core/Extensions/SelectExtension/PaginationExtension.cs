using Minnor.Core.Commands;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Minnor.Core.Extensions.SelectExtension;

public static class PaginationExtension
{
    public static Select<T> Page<T>(this Select<T> query, int pageIndex, int pageSize) where T : class, new()
    {
        if (pageIndex < 0) pageIndex = 0;
        if (pageSize <= 0) pageSize = 10;

        return query.Skip(pageIndex * pageSize).Take(pageSize);
    }

    public static Select<T> Skip<T>(this Select<T> query, int count) where T : class, new()
    {
        var skipProp = typeof(Select<T>).GetField("_skip", BindingFlags.NonPublic | BindingFlags.Instance);
        skipProp?.SetValue(query, count);
        return query;
    }

    public static Select<T> Take<T>(this Select<T> query, int count) where T : class, new()
    {
        var takeProp = typeof(Select<T>).GetField("_take", BindingFlags.NonPublic | BindingFlags.Instance);
        takeProp?.SetValue(query, count);
        return query;
    }
}
