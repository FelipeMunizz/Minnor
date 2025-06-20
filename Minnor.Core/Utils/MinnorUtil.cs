using Minnor.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Minnor.Core.Utils;

public static class MinnorUtil
{
    public static string GetTableName(Type type)
    {
        var tableAttribute = type.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }

    public static string GetColumnName(PropertyInfo type)
    {
        var tableAttribute = type.GetCustomAttribute<ColumnAttribute>();
        return tableAttribute?.Name ?? type.Name;
    }
}
