using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CompiledExpressions;

internal static class Expressions
{
    /// <summary>
    /// Gets the property name and setter expression from a simple property accessor.
    /// Does not support nested properties.
    /// </summary>
    public static (string propertyName, Expression<Action<T, TValue>> assigner) GetSimpleAssigner<T, TValue>(
        Expression<Func<T, TValue>> propertyAccessor
    )
    {
        if (propertyAccessor.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException(
                $"Expression must be a member expression, not {propertyAccessor.Body.NodeType}"
            );
        }

        var propertyInfo = memberExpression.Member as PropertyInfo;
        if (propertyInfo == null)
        {
            throw new ArgumentException(
                $"Expression member must be a property, not {memberExpression.Member.MemberType}"
            );
        }

        var propertyName = propertyInfo.Name;
        var typeParam = Expression.Parameter(typeof(T));
        var valueParam = Expression.Parameter(typeof(TValue));
        var expr = Expression.Lambda<Action<T, TValue>>(
            Expression.Assign(Expression.MakeMemberAccess(typeParam, propertyInfo), valueParam),
            typeParam,
            valueParam
        );
        return (propertyName, expr);
    }

    /// <summary>
    /// Gets the full member path name of a member access expression.
    /// (e.g. "Nested.Text" for `x => x.Nested.Text`)
    /// </summary>
    public static string GetAccessorFullName<T, TValue>(Expression<Func<T, TValue>> memberAccessor)
    {
        var memberNames = EnumerateAccessorMemberInfos(memberAccessor).Select(x => x.Name).Reverse();

        return string.Join(".", memberNames);
    }

    /// <summary>
    /// Gets the member names of a member access expression.
    /// (e.g. ["Nested", "Text"] for `x => x.Nested.Text`)
    /// </summary>
    public static IReadOnlyList<string> GetAccessorMemberNames<T, TValue>(
        Expression<Func<T, TValue>> memberAccessor
    )
    {
        var members = EnumerateAccessorMemberInfos(memberAccessor);

        // Member info names, in reverse order
        return members.Select(x => x.Name).Reverse().ToArray();
    }

    /// <summary>
    /// Enumerates the MemberInfos of a member access expression.
    /// This will be in reverse order, starting from the innermost member.
    /// </summary>
    private static IEnumerable<MemberInfo> EnumerateAccessorMemberInfos<T, TValue>(
        Expression<Func<T, TValue>> memberAccessor
    )
    {
        var currentExpression = memberAccessor.Body;

        // There is a chain of getters in propertyToSet, with at the
        // beginning a ConstantExpression. We put the MemberInfo of
        // these getters in members and the ConstantExpression in ce

        while (currentExpression != null)
        {
            if (currentExpression is MemberExpression memberAccess)
            {
                yield return memberAccess.Member;
                currentExpression = memberAccess.Expression;
            }
            else if (currentExpression is ParameterExpression)
            {
                break;
            }
            else
            {
                throw new NotSupportedException(
                    $"Expression type must be MemberExpression or ParameterExpression: {currentExpression.NodeType}"
                );
            }
        }
    }

    /// <summary>
    /// Gets a setter method from a getter expression.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the property does not have an accessible setter</exception>
    public static Action<T, TValue> GetterToSetterMethod<T, TValue>(
        Expression<Func<T, TValue>> getter,
        bool nonPublic = false
    )
    {
        var result = ParseMemberExpression(getter);

        // Very simple case: p => p.Property or p => p.Field
        if (result.Parameter == result.Instance)
        {
            if (result.MemberAccess.Member.MemberType == MemberTypes.Property)
            {
                var property = (PropertyInfo)result.MemberAccess.Member;

                // This is faster than Expression trees, but works only on public properties
                if (property.GetSetMethod(nonPublic) is { } setter)
                {
                    return (Action<T, TValue>)Delegate.CreateDelegate(typeof(Action<T, TValue>), setter);
                }

                // GetSetMethod was null, error if nonPublic is false
                // otherwise continue to making an expression tree
                if (!nonPublic)
                {
                    throw new InvalidOperationException(
                        $"Expression member {property.Name} must have an accessible setter"
                    );
                }
            }
        }

        // Check that the final member access has public setter
        if (!nonPublic && result.MemberAccess.Member.MemberType == MemberTypes.Property)
        {
            var property = (PropertyInfo)result.MemberAccess.Member;
            if (property.GetSetMethod(nonPublic) is null)
            {
                throw new InvalidOperationException(
                    $"Expression member {property.Name} must have an accessible setter"
                );
            }
        }

        var value = Expression.Parameter(typeof(TValue), "val");

        var expr = Expression.Assign(result.MemberAccess, value);

        return Expression.Lambda<Action<T, TValue>>(expr, result.Parameter, value).Compile();
    }

    /// <summary>
    /// Gets a setter expression from a getter expression.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static Expression<Action<T, TValue>> GetterToSetter<T, TValue>(
        Expression<Func<T, TValue>> getter,
        bool nonPublic = false
    )
    {
        var result = ParseMemberExpression(getter);

        // Check that the final member access has public setter
        if (!nonPublic && result.MemberAccess.Member.MemberType == MemberTypes.Property)
        {
            var property = (PropertyInfo)result.MemberAccess.Member;
            if (property.GetSetMethod(nonPublic) is null)
            {
                throw new InvalidOperationException(
                    $"Expression member {property.Name} must have an accessible setter"
                );
            }
        }

        var value = Expression.Parameter(typeof(TValue), "val");

        var expr = Expression.Assign(result.MemberAccess, value);

        return Expression.Lambda<Action<T, TValue>>(expr, result.Parameter, value);
    }

    /// <summary>
    /// Parses a member access expression into its components
    /// </summary>
    /// <param name="expression"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static (
        ParameterExpression Parameter,
        Expression Instance,
        MemberExpression MemberAccess
    ) ParseMemberExpression<T, TValue>(Expression<Func<T, TValue>> expression)
    {
        var currentExpression = expression.Body;

        while (currentExpression.NodeType is ExpressionType.Convert or ExpressionType.TypeAs)
        {
            currentExpression = ((UnaryExpression)currentExpression).Operand;
        }

        // Get the full access chain to the last member
        // e.g. `x.Property1.Property2.Property3`
        if (currentExpression is not MemberExpression memberAccess)
        {
            throw new ArgumentException(
                $"Expression body type must be MemberAccess, not {currentExpression.NodeType}: {currentExpression}",
                nameof(expression)
            );
        }

        // Get the expression for the instance of the last member
        // e.g. `x.Property1.Property2`
        if (memberAccess.Expression is not { } instanceExpression)
        {
            throw new ArgumentException(
                $"Expression member must have a non-null expression: {memberAccess}",
                nameof(expression)
            );
        }

        currentExpression = instanceExpression;

        while (currentExpression.NodeType != ExpressionType.Parameter)
        {
            currentExpression = currentExpression.NodeType switch
            {
                // Get Operand of any Convert or TypeAs expressions
                ExpressionType.Convert
                or ExpressionType.TypeAs
                    => ((UnaryExpression)currentExpression).Operand,
                ExpressionType.MemberAccess
                    => ((MemberExpression)currentExpression).Expression
                        ?? throw new ArgumentException(
                            $"Expression member must have a non-null expression: {currentExpression}",
                            nameof(expression)
                        ),
                _
                    => throw new ArgumentException(
                        $"Expression member type must be MemberAccess or Parameter, not {currentExpression.NodeType}: {currentExpression}",
                        nameof(expression)
                    )
            };
        }

        if (currentExpression is not ParameterExpression parameterExpression)
        {
            throw new ArgumentException(
                $"Expression member must have a parameter: {currentExpression}",
                nameof(expression)
            );
        }

        return (parameterExpression, instanceExpression, memberAccess);
    }
}
