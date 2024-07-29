using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CompiledExpressions.Sample;

/// <summary>
/// Sample class to demonstrate the usage of CompiledExpressions.
/// Functions for creating relays of property changes between INotifyPropertyChanged objects.
/// </summary>
public static class ObservableRelay
{
    /// <summary>
    /// Creates a one-way relay of property changes from the source object to the target object.
    /// </summary>
    public static IDisposable OneWay<TSource, TTarget, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> sourceProperty,
        TTarget target,
        Expression<Func<TTarget, TValue>> targetProperty
    )
        where TSource : INotifyPropertyChanged
    {
        var sourceAccessor = CompiledExpression.CreateAccessor(sourceProperty).WithInstance(source);
        var targetAccessor = CompiledExpression.CreateAccessor(targetProperty).WithInstance(target);
        
        return new PropertyChangedEventSubscription(source, (_, e) =>
        {
            if (e.PropertyName == sourceAccessor.MemberNames[0])
            {
                targetAccessor.Set(sourceAccessor.Get());
            }
        });
    }

    private sealed class PropertyChangedEventSubscription : IDisposable
    {
        private readonly INotifyPropertyChanged _source;
        private readonly PropertyChangedEventHandler _handler;

        public PropertyChangedEventSubscription(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            _source = source;
            _handler = handler;
            
            _source.PropertyChanged += _handler;
        }

        public void Dispose()
        {
            _source.PropertyChanged -= _handler;
        }
    }
}
