using System;
using System.Linq.Expressions;

namespace SpanJson.ChangedBySP
{
    // changed by SP: added
    public static class Changes
    {
        public static MemberExpression GetMemberExpressionForPropertyOrField(Expression expression, string propertyOrFieldName)
        {
            var property = expression.Type.GetProperty(propertyOrFieldName);
            if (property != null)
                return System.Linq.Expressions.Expression.Property(expression, property);

            var field = expression.Type.GetField(propertyOrFieldName);
            if (field != null)
                return System.Linq.Expressions.Expression.Field(expression, field);

            throw new Exception($"Please, check code. Can't find '{propertyOrFieldName}' member in {expression.Type.Name} type.");
        }
    }
}
