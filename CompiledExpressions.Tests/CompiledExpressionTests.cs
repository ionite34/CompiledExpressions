using CompiledExpressions.Tests.Models;

namespace CompiledExpressions.Tests;

[TestClass]
public class CompiledExpressionTests
{
    [TestMethod]
    public void CreateAccessor_Expression()
    {
        var testObj = new TestClass { Text = "abc" };
        
        var accessor = CompiledExpression.CreateAccessor<TestClass, string?>(x => x.Text);

        Assert.AreEqual("Text", accessor.FullName);
        Assert.AreEqual("abc", accessor.Get(testObj));
        
        accessor.Set(testObj, "def");
        
        Assert.AreEqual("def", testObj.Text);
    }

    [TestMethod]
    public void CreateAccessor_GetSetExpression()
    {
        var testObj = new TestClass { Text = "abc" };
        
        var accessor = CompiledExpression.CreateAccessor<TestClass, string?>(
            x => x.Text,
            (x, val) => x.Text = val
        );

        Assert.AreEqual("Text", accessor.FullName);
        Assert.AreEqual("abc", accessor.Get(testObj));
        
        accessor.Set(testObj, "def");
        
        Assert.AreEqual("def", testObj.Text);
    }
}
