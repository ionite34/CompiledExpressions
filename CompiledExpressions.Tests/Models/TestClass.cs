namespace CompiledExpressions.Tests.Models;

public class TestClass
{
    public int Id { get; set; }
    
    public int PrivateGetId { get; private set; }
    
    public string? Text { get; set; }
    
    public TestClass? Nested { get; set; }

    /// <summary>
    /// Create a nested TestClass with the specified depth.
    /// </summary>
    public static TestClass CreateNested(int depth)
    {
        var obj = new TestClass();
        var current = obj;
        
        for (var i = 0; i < depth; i++)
        {
            current.Nested = new TestClass();
            current = current.Nested;
        }

        return obj;
    }
}
