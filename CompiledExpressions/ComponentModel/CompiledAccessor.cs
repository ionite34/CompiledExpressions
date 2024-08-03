using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CompiledExpressions.ComponentModel;

/// <summary>
/// Represents Getter and Setter accessors for a property or field. 
/// </summary>
/// <param name="getter">Get Function accepting a root instance and returning the property or field value.</param>
/// <param name="setter">Set Action accepting a root instance and the new property or field value.</param>
/// <typeparam name="T">Type of the root instance</typeparam>
/// <typeparam name="TValue">Type of the property or field</typeparam>
public class CompiledAccessor<T, TValue>(Func<T, TValue> getter, Action<T, TValue> setter)
{
    /// <summary>
    /// Getter function
    /// </summary>
    private protected readonly Func<T, TValue> Getter = getter;
    
    /// <summary>
    /// Setter action
    /// </summary>
    private protected readonly Action<T, TValue> Setter = setter;

    /// <summary>
    /// Names of the members in the accessor path. (e.g. ["Nested", "Text"] for `x => x.Nested.Text`)
    /// </summary>
    public IReadOnlyList<string> MemberNames { get; init; } = [];
    
    /// <summary>
    /// Full name of the accessor path. (e.g. "Nested.Text" for `x => x.Nested.Text`)
    /// </summary>
    public string FullName => string.Join(".", MemberNames);

    /// <summary>
    /// Gets the value of the property or field.
    /// </summary>
    /// <param name="instance">Root instance</param>
    /// <returns>Property or field value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue Get(T instance) => Getter(instance);

    /// <summary>
    /// Sets the value of the property or field.
    /// </summary>
    /// <param name="instance">Root instance</param>
    /// <param name="value">New value to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(T instance, TValue value) => Setter(instance, value);

    /// <summary>
    /// Creates a CompiledInstanceAccessor with the specified instance and the existing getter and setter.
    /// </summary>
    /// <param name="instance">Root instance</param>
    /// <returns>New CompiledInstanceAccessor</returns>
    public CompiledInstanceAccessor<T, TValue> WithInstance(T instance) => new(instance, Getter, Setter)
    {
        MemberNames = MemberNames
    };
}
