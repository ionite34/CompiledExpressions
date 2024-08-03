using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CompiledExpressions.ComponentModel;

namespace CompiledExpressions.Benchmarks;

public class AccessorBenchmark
{
    private Expression<Func<TestClass, int>> getExpression = null!;
    private Expression<Func<TestClass, int>> getDepth2Expression = null!;
    private Expression<Func<TestClass, int>> getDepth4Expression = null!;
    
    [IterationSetup]
    public void IterationSetup()
    {
        getExpression = x => x.Value;
        getDepth2Expression = x => x.Nested!.Value;
        getDepth4Expression = x => x.Nested!.Nested!.Nested!.Value;
    }

    [Benchmark]
    public CompiledAccessor<TestClass, int> CompiledAccessor_Create()
    {
        return CompiledExpression.CreateAccessor(getExpression);
    }
    
    [Benchmark]
    public CompiledAccessor<TestClass, int> CompiledAccessor_Create_Depth2()
    {
        return CompiledExpression.CreateAccessor(getDepth2Expression);
    }
    
    [Benchmark]
    public CompiledAccessor<TestClass, int> CompiledAccessor_Create_Depth4()
    {
        return CompiledExpression.CreateAccessor(getDepth4Expression);
    }
}
