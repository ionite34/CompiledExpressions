using System;
using System.Runtime.CompilerServices;

namespace CompiledExpressions.ComponentModel;

/// <summary>
/// Represents Getter and Setter accessors for a property or field, with a reference to the root instance.
/// </summary>
/// <typeparam name="T">Type of the root instance</typeparam>
/// <typeparam name="TValue">Type of the property or field</typeparam>
public class CompiledInstanceAccessor<T, TValue> : CompiledAccessor<T, TValue>
{
    private readonly T _instance;

    /// <summary>
    /// Creates a CompiledInstanceAccessor with the specified instance and the existing getter and setter.
    /// </summary>
    /// <param name="instance">Root instance</param>
    /// <param name="getter">Get Function accepting a root instance and returning the property or field value.</param>
    /// <param name="setter">Set Action accepting a root instance and the new property or field value.</param>
    public CompiledInstanceAccessor(T instance, Func<T, TValue> getter, Action<T, TValue> setter)
        : base(getter, setter)
    {
        _instance = instance;
    }

    /// <summary>
    /// Gets the value of the property or field using the instance.
    /// </summary>
    /// <returns>Property or field value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue Get() => Getter(_instance);

    /// <summary>
    /// Sets the value of the property or field on the instance.
    /// </summary>
    /// <param name="value">New value to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TValue value) => Setter(_instance, value);
}
