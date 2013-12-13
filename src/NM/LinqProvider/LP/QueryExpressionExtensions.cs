using System;
using System.Linq.Expressions;
using IQToolkit;

namespace IQToolkit
{
    static class QueryExpressionExtensions
    {
        public static ConstantExpression GetQueryConstantExpression(this Expression expression)
        {
            ConstantExpression result = null;
            dynamic dynamicExpression = (dynamic)expression;
            while (dynamicExpression.Arguments.Count > 0)
            {
                var currentExpression = dynamicExpression.Arguments[0];
                if (currentExpression.NodeType == ExpressionType.Constant)
                {
                    result = ((ConstantExpression)currentExpression);
                    if (result.Value.GetType().GetGenericTypeDefinition() == typeof(Query<>))
                        break;
                    else
                        result = null;
                }
                dynamicExpression = dynamicExpression.Arguments[0];
            }

            return result;
        }

        public static Type GetElementType(this Expression expression)
        {
            var queryConstantExpression = expression.GetQueryConstantExpression();
            if (queryConstantExpression != null)
                return queryConstantExpression.Value.GetType().GetGenericArguments()[0];
            else
                return TypeHelper.GetElementType(expression.Type);
        }
    }
}
