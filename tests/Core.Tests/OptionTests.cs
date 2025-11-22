using EHonda.Optional.Core;

namespace EHonda.Optional.Core.Tests;

public class OptionTests
{
    [Test]
    public async Task Default_Option_Should_Be_None()
    {
        Option<int> option = default;
        
        await Assert.That(option.HasValue).IsFalse();
        await Assert.That(option.Value).IsEqualTo(0); // default(int)
    }

    [Test]
    public async Task Option_None_Should_Be_Equivalent_To_Default()
    {
        var none = Option.None<int>();
        Option<int> def = default;

        await Assert.That(none).IsEqualTo(def);
        await Assert.That(none.HasValue).IsFalse();
    }

    [Test]
    public async Task Option_Some_Should_Have_Value()
    {
        var option = Option.Some(42);

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Option_Some_Null_Should_Throw()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
        {
            Option.Some<string?>(null);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Equality_Two_Defaults_Should_Be_Equal()
    {
        Option<int> a = default;
        Option<int> b = default;

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Equality_Some_And_None_Should_Not_Be_Equal()
    {
        var some = Option.Some(5);
        Option<int> none = default;

        await Assert.That(some).IsNotEqualTo(none);
    }

    [Test]
    public async Task Equality_Two_Somes_With_Same_Value_Should_Be_Equal()
    {
        var a = Option.Some(5);
        var b = Option.Some(5);

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Equality_Two_Somes_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = Option.Some(5);
        var b = Option.Some(6);

        await Assert.That(a).IsNotEqualTo(b);
    }

    [Test]
    public async Task Implicit_Conversion_From_Value_Should_Create_Some()
    {
        Option<int> option = 42;

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsEqualTo(42);
    }
    
    private interface IService
    {
        int GetValue();
    }

    private class Service : IService
    {
        public int GetValue() => 42;
    }

    [Test]
    public async Task Implicit_Conversion_From_Class_To_Interface_Should_Create_Some()
    {
        // Implicit conversion from IService does not work, which is why we don't test for it.
        // See limitations in README.md
        Option<IService> option = new Service();

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value!.GetValue()).IsEqualTo(42);
    }

    [Test]
    public async Task Implicit_Conversion_From_Null_Should_Throw()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
        {
            Option<string?> option = null;
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Explicit_Conversion_To_Value_Should_Return_Value()
    {
        var option = Option.Some(42);
        int value = (int)option;

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Explicit_Conversion_From_None_Should_Throw()
    {
        Option<int> none = default;

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
        {
            var _ = (int)none;
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Or_Value_Should_Return_Content_Or_Fallback()
    {
        await Assert.That(Option.Some(5).Or(10)).IsEqualTo(5);
        await Assert.That(Option.None<int>().Or(10)).IsEqualTo(10);
    }

    [Test]
    public async Task Or_Factory_Should_Return_Content_Or_Invoke_Factory()
    {
        await Assert.That(Option.Some(5).Or(() => 10)).IsEqualTo(5);
        await Assert.That(Option.None<int>().Or(() => 10)).IsEqualTo(10);
    }

    [Test]
    public async Task OrDefault_Should_Return_Content_Or_Type_Default()
    {
        await Assert.That(Option.Some(5).OrDefault()).IsEqualTo(5);
        await Assert.That(Option.None<int>().OrDefault()).IsEqualTo(0);
    }

    [Test]
    public async Task OrThrow_Should_Return_Value_Or_Throw()
    {
        await Assert.That(Option.Some(5).OrThrow()).IsEqualTo(5);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
        {
             Option.None<int>().OrThrow();
             await Task.CompletedTask;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
        {
             Option.None<int>().OrThrow("Custom message");
             await Task.CompletedTask;
        });
        
        await Assert.ThrowsAsync<ArgumentException>(async () => 
        {
             Option.None<int>().OrThrow(() => new ArgumentException());
             await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Deconstruct_Should_Support_Pattern_Matching()
    {
        var some = Option.Some(42);
        if (some is (true, var val))
        {
            await Assert.That(val).IsEqualTo(42);
        }
        else
        {
            Assert.Fail("Should have matched Some pattern");
        }

        var none = Option.None<int>();
        if (none is (false, _))
        {
            // Success
        }
        else
        {
            Assert.Fail("Should have matched None pattern");
        }
    }
}
