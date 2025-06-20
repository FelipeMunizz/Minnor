using Minnor.Core.Attributes;
using Minnor.Core.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Minnor.Core.Sql;

public class SqlExpressionVisitor : ExpressionVisitor
{
    public StringBuilder _sb = new();

    public string Translate(Expression expression)
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
        _sb.Append(node.Member.Name);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Type == typeof(string))
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
}
