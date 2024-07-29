## CompiledExpressions
[![Build](https://github.com/ionite34/CompiledExpressions/actions/workflows/build.yml/badge.svg)](https://github.com/ionite34/CompiledExpressions/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/ionite34/CompiledExpressions/branch/main/graph/badge.svg?token=Uyd765s2KE)](https://codecov.io/gh/ionite34/CompiledExpressions)

Create Compiled Delegates like Setters from Getter expressions
- Any level of nesting member support (i.e. `x => x.Prop1.Prop2.Prop3`)
- Bind time error and accessibility checking. (i.e. Getters without a Setter or with a private Setter)
- NativeAOT compatible with no run-time reflection.

## Usage
> Full Sample Project: [CompiledExpressions.Sample](./CompiledExpressions.Sample)
 

1. For a library method, call `CompiledExpression.CreateAccessor` with a property/field access expression. This returns a [`CompiledAccessor`](CompiledExpressions/ComponentModel/CompiledAccessor.cs) struct with `Get<T, TValue>(T instance)` and `Set<T, TValue>(T instance, TValue value)` methods. The `MemberNames` property contains an array of the member names in the original expression.
```csharp
using System;
using System.Linq.Expressions;
using CompiledExpressions;


public static class Example
{
    public static void Bind<T, TValue>(
        T target,
        Expression<Func<T, TValue>> targetMember
    )
    {
        var accessor = CompiledExpression.CreateAccessor(targetMember);
        
        // Get the existing value
        var value = accessor.Get(target);
        
        // Set a new value
        accessor.Set(target, value);
        
        // Get the member names
        var memberNames = accessor.MemberNames; // ["Prop1", "Prop2", "Prop3"]
        
        var memberPath = accessor.FullName; // "Prop1.Prop2.Prop3"
    }
}
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
