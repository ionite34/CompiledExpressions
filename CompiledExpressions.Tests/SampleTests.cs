using CompiledExpressions.Sample;
using CompiledExpressions.Tests.Models;

namespace CompiledExpressions.Tests;

[TestClass]
public class SampleTests
{
    [TestMethod]
    public void TestOneWayRelay()
    {
        var source = new ObservableTestClass();
        var target = new TestClass();

        using var idSubscription = ObservableRelay.OneWay(source, x => x.Id, target, x => x.Id);
        using var textSubscription = ObservableRelay.OneWay(source, x => x.Text, target, x => x.Text);

        Assert.AreEqual(0, target.Id);
        Assert.AreEqual(null, target.Text);

        source.Id = 42;
        source.Text = "Hello";

        Assert.AreEqual(42, target.Id);
        Assert.AreEqual("Hello", target.Text);
    }
}
