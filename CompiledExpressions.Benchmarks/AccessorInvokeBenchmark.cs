using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CompiledExpressions.ComponentModel;

namespace CompiledExpressions.Benchmarks;

public class AccessorInvokeBenchmark
{
    private TestClass testObj = null!;
    
    private Expression<Func<TestClass, int>> getExpression = null!;
    private CompiledAccessor<TestClass, int> accessor = null!;

    private PropertyInfo propertyInfo = null!;
        
    [IterationSetup(Target = nameof(CompiledAccessor_Set))]
    public void IterationSetup_CompiledAccessor_Set()
    {
        testObj = new TestClass
        {
            Value = 123,
            Nested = new TestClass { Value = 456 }
        };

        getExpression = x => x.Nested!.Value;

        accessor = CompiledExpression.CreateAccessor(getExpression);
    }
    
    [IterationSetup(Target = nameof(ReflectionAccessor_Set))]
    public void IterationSetup_ReflectionAccessor_Set()
    {
        testObj = new TestClass
        {
            Value = 123,
            Nested = new TestClass { Value = 456 }
        };

        propertyInfo =
            typeof(TestClass).GetProperty(nameof(TestClass.Value)) ?? throw new MissingMemberException();
    }

    [Benchmark]
    public void CompiledAccessor_Set()
    {
        accessor.Set(testObj, 0);
        accessor.Set(testObj, 123);
    }

    [Benchmark]
    public void ReflectionAccessor_Set()
    {
        propertyInfo.SetValue(testObj, 0);
        propertyInfo.SetValue(testObj, 123);
    }
}
