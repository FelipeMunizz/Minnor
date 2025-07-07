using Minnor.Core.Context;

namespace Minnor.Core.Extensions.ContextExtension;

public static class MinnorContextExtensions
{
    public static IEnumerable<T> InsertRange<T>(this MinnorContext context, IEnumerable<T> entities) where T : class, new()
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (entities is null)
            throw new ArgumentNullException(nameof(entities));

        var results = new List<T>();

        foreach (var entity in entities)
        {
            var result = context.Insert(entity);
            results.Add(result);
        }

        return results;
    }
}
