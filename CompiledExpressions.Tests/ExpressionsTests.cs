using CompiledExpressions.Tests.Models;

namespace CompiledExpressions.Tests;

[TestClass]
public class ExpressionsTests
{
    [TestMethod]
    public void GetAssigner_Simple_PropertyName()
    {
        var (propertyName, _) = Expressions.GetAssigner<TestClass, int>(x => x.Id);

        // Check that the property name is correct
        Assert.AreEqual("Id", propertyName);
    }

    [TestMethod]
    public void GetAssigner_Simple_PropertyAssignment()
    {
        var obj = new TestClass();

        var (_, assigner) = Expressions.GetAssigner<TestClass, int>(x => x.Id);

        assigner.Compile()(obj, 42);

        Assert.AreEqual(42, obj.Id);
    }

    [TestMethod]
    public void GetAccessorNames_Property()
    {
        var memberNames = Expressions.GetAccessorMemberNames<TestClass, int>(x => x.Id).ToList();

        // Check that the property name is correct
        Assert.AreEqual(1, memberNames.Count);
        Assert.AreEqual("Id", memberNames[0]);
    }

    [TestMethod]
    public void GetAccessorNames_NestedProperty()
    {
        var memberNames = Expressions
            .GetAccessorMemberNames<TestClass, string?>(x => x.Nested!.Text)
            .ToList();

        // Check that the property name is correct
        Assert.AreEqual(2, memberNames.Count);
        Assert.AreEqual("Nested", memberNames[0]);
        Assert.AreEqual("Text", memberNames[1]);
    }

    [TestMethod]
    public void GetAccessorFullName_Property()
    {
        var propertyName = Expressions.GetAccessorFullName<TestClass, int>(x => x.Id);

        // Check that the property name is correct
        Assert.AreEqual("Id", propertyName);
    }

    [TestMethod]
    public void GetAccessorFullName_NestedProperty()
    {
        var propertyName = Expressions.GetAccessorFullName<TestClass, string?>(x => x.Nested!.Text);

        // Check that the property name is correct
        Assert.AreEqual("Nested.Text", propertyName);
    }

    [TestMethod]
    public void GetterToSetter_Property()
    {
        var obj = new TestClass();

        var setter = Expressions.GetterToSetterMethod<TestClass, int>(x => x.Id);

        setter(obj, 42);

        Assert.AreEqual(42, obj.Id);
    }

    [TestMethod]
    public void GetterToSetter_NestedProperty()
    {
        var obj = new TestClass { Nested = new TestClass() };

        var setter = Expressions.GetterToSetterMethod<TestClass, string?>(x => x.Nested!.Text);

        setter(obj, "Hello");

        Assert.AreEqual("Hello", obj.Nested!.Text);
    }

    [TestMethod]
    public void GetterToSetter_NonPublicProperty_ShouldThrow()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            _ = Expressions.GetterToSetterMethod<TestClass, int>(x => x.PrivateGetId);
        });
    }

    [TestMethod]
    public void GetterToSetter_NestedNonPublicProperty_ShouldThrow()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            _ = Expressions.GetterToSetterMethod<TestClass, int>(x => x.Nested!.PrivateGetId);
        });
    }

    [TestMethod]
    public void GetterToSetter_Constant_ShouldThrow()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = Expressions.GetterToSetterMethod<TestClass, int>(_ => 123);
        });

        Assert.ThrowsException<ArgumentException>(() =>
        {
            _ = Expressions.GetterToSetterMethod<TestClass, int>(_ => new TestClass().Id);
        });
    }

    [TestMethod]
    public void GetterToSetter_NonPublic_Property()
    {
        var testObj = new TestClass();

        var setter = Expressions.GetterToSetterMethod<TestClass, int>(x => x.PrivateGetId, true);

        setter(testObj, 123);

        Assert.AreEqual(123, testObj.PrivateGetId);
    }

    [TestMethod]
    public void GetterToSetter_NonPublic_NestedProperty()
    {
        var testObj = new TestClass { Nested = new TestClass() };

        var setter = Expressions.GetterToSetterMethod<TestClass, int>(x => x.Nested!.PrivateGetId, true);

        setter(testObj, 123);

        Assert.AreEqual(123, testObj.Nested.PrivateGetId);
    }
}
