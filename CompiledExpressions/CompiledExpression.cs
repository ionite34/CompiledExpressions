using System;
using System.Linq;
using System.Linq.Expressions;
using CompiledExpressions.ComponentModel;
using JetBrains.Annotations;

namespace CompiledExpressions;

/// <summary>
/// Provides methods to compile expressions into delegates
/// </summary>
[PublicAPI]
public class CompiledExpression
{
    private CompiledExpression()
    {
    }
    
    /// <summary>
    /// Creates a CompiledAccessor from a property getter expression.
    /// </summary>
    /// <param name="expression">Property accessor expression</param>
    /// <typeparam name="T">Type of the root instance</typeparam>
    /// <typeparam name="TValue">Type of the accessed property or field</typeparam>
    /// <returns>New CompiledAccessor</returns>
    public static CompiledAccessor<T, TValue> CreateAccessor<T, TValue>(
        Expression<Func<T, TValue>> expression
    )
    {
        return new CompiledAccessor<T, TValue>(
            expression.Compile(),
            Expressions.GetterToSetterMethod(expression)
        )
        {
            MemberNames = Expressions.GetAccessorMemberNames(expression).ToArray()
        };
    }
    
    /// <summary>
    /// Creates a CompiledAccessor from a property getter and setter expression.
    /// </summary>
    /// <param name="getExpression">Property getter expression</param>
    /// <param name="setAction">Property setter action</param>
    /// <typeparam name="T">Type of the root instance</typeparam>
    /// <typeparam name="TValue">Type of the accessed property or field</typeparam>
    /// <returns>New CompiledAccessor</returns>
    public static CompiledAccessor<T, TValue> CreateAccessor<T, TValue>(
        Expression<Func<T, TValue>> getExpression,
        Action<T, TValue> setAction
    )
    {
        return new CompiledAccessor<T, TValue>(
            getExpression.Compile(),
            setAction
        )
        {
            MemberNames = Expressions.GetAccessorMemberNames(getExpression).ToArray()
        };
    }
}
