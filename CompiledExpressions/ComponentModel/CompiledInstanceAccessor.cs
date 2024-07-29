using System;
using System.Runtime.CompilerServices;

namespace CompiledExpressions.ComponentModel;

/// <summary>
/// Represents Getter and Setter accessors for a property or field, with a reference to the root instance.
/// </summary>
/// <typeparam name="T">Type of the root instance</typeparam>
/// <typeparam name="TValue">Type of the property or field</typeparam>
public readonly struct CompiledInstanceAccessor<T, TValue>
{
    private readonly T _instance;
    private readonly Func<T, TValue> _getter;
    private readonly Action<T, TValue> _setter;

    /// <inheritdoc cref="CompiledAccessor{T,TValue}.MemberNames"/>
    public string[] MemberNames { get; init; } = [];

    /// <inheritdoc cref="CompiledAccessor{T,TValue}.FullName"/>
    public string FullName => string.Join(".", MemberNames);

    /// <summary>
    /// Creates a CompiledInstanceAccessor with the specified instance and the existing getter and setter.
    /// </summary>
    /// <param name="instance">Root instance</param>
    /// <param name="getter">Get Function accepting a root instance and returning the property or field value.</param>
    /// <param name="setter">Set Action accepting a root instance and the new property or field value.</param>
    public CompiledInstanceAccessor(T instance, Func<T, TValue> getter, Action<T, TValue> setter)
    {
        _instance = instance;
        _getter = getter;
        _setter = setter;
    }

    /// <summary>
    /// Gets the value of the property or field using the instance.
    /// </summary>
    /// <returns>Property or field value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue Get() => _getter(_instance);

    /// <inheritdoc cref="CompiledAccessor{T,TValue}.Get"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue Get(T instance) => _getter(instance);


    /// <summary>
    /// Sets the value of the property or field on the instance.
    /// </summary>
    /// <param name="value">New value to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TValue value) => _setter(_instance, value);

    /// <inheritdoc cref="CompiledAccessor{T,TValue}.Set"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(T instance, TValue value) => _setter(instance, value);
}
