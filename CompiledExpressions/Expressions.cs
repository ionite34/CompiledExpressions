using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CompiledExpressions;

internal static class Expressions
{
    public static (string propertyName, Expression<Action<T, TValue>> assigner) GetAssigner<T, TValue>(
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
    /// Enumerates the MemberInfos of a property access expression
    /// </summary>
    private static IEnumerable<MemberInfo> EnumerateMemberInfos<T, TValue>(
        Expression<Func<T, TValue>> propertyAccessor
    )
    {
        var currentExpression = propertyAccessor.Body;

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
    /// Gets the full name or path of a member access expression
    /// </summary>
    public static string GetAccessorFullName<T, TValue>(Expression<Func<T, TValue>> propertyAccessor)
    {
        return string.Join(".", GetAccessorMemberNames(propertyAccessor));
    }

    /// <summary>
    /// Gets the member names of a member access expression
    /// </summary>
    public static IEnumerable<string> GetAccessorMemberNames<T, TValue>(
        Expression<Func<T, TValue>> propertyAccessor
    )
    {
        var members = EnumerateMemberInfos(propertyAccessor);

        // Member info names, in reverse order
        return members.Select(x => x.Name).Reverse();
    }

    public static Action<T, TValue> GetterToSetterMethod<T, TValue>(
        Expression<Func<T, TValue>> getter,
        bool nonPublic = false
    )
    {
        var (parameter, instance, memberAccess) = ParseMemberExpression(getter);

        // Very simple case: p => p.Property or p => p.Field
        if (parameter == instance)
        {
            if (memberAccess.Member.MemberType == MemberTypes.Property)
            {
                // This is faster than Expression trees, but works only on public properties
                var property = (PropertyInfo)memberAccess.Member;

                if (property.GetSetMethod(nonPublic) is { } setter)
                {
                    var action =
                        (Action<T, TValue>)Delegate.CreateDelegate(typeof(Action<T, TValue>), setter);
                    return action;
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
        if (!nonPublic && memberAccess.Member.MemberType == MemberTypes.Property)
        {
            var property = (PropertyInfo)memberAccess.Member;
            if (property.GetSetMethod(nonPublic) is null)
            {
                throw new InvalidOperationException(
                    $"Expression member {property.Name} must have an accessible setter"
                );
            }
        }
        
        var value = Expression.Parameter(typeof(TValue), "val");

        var expr = Expression.Assign(memberAccess, value);

        return Expression.Lambda<Action<T, TValue>>(expr, parameter, value).Compile();
    }

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

    private static (
        ParameterExpression Parameter,
        Expression Instance,
        MemberExpression MemberAccess
    ) ParseMemberExpression<T, TValue>(Expression<Func<T, TValue>> expression)
    {
        var currentExpression = expression.Body;

        while (currentExpression.NodeType is ExpressionType.Convert or ExpressionType.TypeAs)
        {
            currentExpression = (currentExpression as UnaryExpression)!.Operand;
        }

        if (currentExpression.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException(
                $"Expression body type must be a member access, not {currentExpression.NodeType}"
            );
        }

        // Get the initial member access
        if (currentExpression is not MemberExpression memberAccess)
        {
            throw new ArgumentException("Expression must access a member property or field");
        }

        if (memberAccess.Expression is not { } instanceExpression)
        {
            throw new ArgumentException("Expression member must have a non-null expression");
        }

        currentExpression = instanceExpression;

        while (currentExpression is not null && currentExpression.NodeType != ExpressionType.Parameter)
        {
            currentExpression = currentExpression.NodeType switch
            {
                ExpressionType.Convert
                or ExpressionType.TypeAs
                    => (currentExpression as UnaryExpression)!.Operand,
                ExpressionType.MemberAccess => (currentExpression as MemberExpression)!.Expression,
                _
                    => throw new ArgumentException(
                        $"Expression member must be a parameter, not {currentExpression.NodeType}"
                    )
            };
        }

        if (currentExpression is not ParameterExpression parameterExpression)
        {
            throw new ArgumentException("Expression member must have a parameter");
        }

        return (parameterExpression, instanceExpression, memberAccess);
    }
}
