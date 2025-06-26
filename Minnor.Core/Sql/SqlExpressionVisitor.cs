using Minnor.Core.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Minnor.Core.Sql;

internal class SqlExpressionVisitor : ExpressionVisitor
{
    internal StringBuilder _sb = new();

    internal string Translate(Expression expression)
    {
        Visit(expression);
        return $"WHERE {_sb.ToString()}";
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _sb.Append("(");
        Visit(node.Left);
        _sb.Append($" {ToSqlOperator(node.NodeType)}");
        Visit(node.Right);
        _sb.Append(")");

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is not null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            var propertyInfo = node.Member as PropertyInfo;
            if (propertyInfo is not null)
            {
                _sb.Append(MinnorUtil.GetColumnName(propertyInfo));
                return node;
            }
        }

        var value = GetValue(node);
        if (value is string || value is DateTime)
            _sb.Append($"'{value}'");
        else
            _sb.Append(value);

        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Type == typeof(string) || node.Type == typeof(DateTime))
            _sb.Append($"'{node.Value}'");
        else
            _sb.Append(node.Value);
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name == "Contains" && node.Object != null)
        {
            Visit(node.Object);
            _sb.Append(" LIKE ");
            var arg = (ConstantExpression)node.Arguments[0];
            _sb.Append($"'%{arg.Value}%'");
        }

        return node;
    }

    private static string ToSqlOperator(ExpressionType type) => type switch
    {
        ExpressionType.Equal => "=",
        ExpressionType.NotEqual => "<>",
        ExpressionType.GreaterThan => ">",
        ExpressionType.LessThan => "<",
        ExpressionType.GreaterThanOrEqual => ">=",
        ExpressionType.LessThanOrEqual => "<=",
        ExpressionType.AndAlso => "AND",
        ExpressionType.OrElse => "OR",
        _ => throw new NotSupportedException($"Operador {type} não suportado")
    };

    private static object? GetValue(MemberExpression member)
    {
        if (member.Expression is ConstantExpression constant)
        {
            var fieldInfo = member.Member as FieldInfo;
            return fieldInfo?.GetValue(constant.Value);
        }

        if (member.Expression is MemberExpression innerMember)
        {
            var innerValue = GetValue(innerMember);
            if (innerValue == null) return null;

            if (member.Member is FieldInfo fi)
                return fi.GetValue(innerValue);
            if (member.Member is PropertyInfo pi)
                return pi.GetValue(innerValue);
        }

        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        return getterLambda.Compile().Invoke();
    }
}
