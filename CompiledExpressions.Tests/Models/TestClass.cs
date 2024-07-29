namespace CompiledExpressions.Tests.Models;

public class TestClass
{
    public int Id { get; set; }
    
    public int PrivateGetId { get; private set; }
    
    public string? Text { get; set; }
    
    public TestClass? Nested { get; set; }
}
