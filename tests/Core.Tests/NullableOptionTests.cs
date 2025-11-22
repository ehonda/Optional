using EHonda.Optional.Core;

namespace EHonda.Optional.Core.Tests;

public class NullableOptionTests
{
    [Test]
    public async Task Default_NullableOption_Should_Be_None()
    {
        NullableOption<int> option = default;
        
        await Assert.That(option.HasValue).IsFalse();
        await Assert.That(option.Value).IsEqualTo(0); // default(int)
    }

    [Test]
    public async Task NullableOption_None_Should_Be_Equivalent_To_Default()
    {
        var none = NullableOption.None<int>();
        NullableOption<int> def = default;

        await Assert.That(none).IsEqualTo(def);
        await Assert.That(none.HasValue).IsFalse();
    }

    [Test]
    public async Task NullableOption_Some_Should_Have_Value()
    {
        var option = NullableOption.Some(42);

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsEqualTo(42);
    }

    [Test]
    public async Task NullableOption_Some_Null_Should_Be_Some()
    {
        var option = NullableOption.Some<string?>(null);

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsNull();
    }

    [Test]
    public async Task Equality_Two_Defaults_Should_Be_Equal()
    {
        NullableOption<int> a = default;
        NullableOption<int> b = default;

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Equality_Some_And_None_Should_Not_Be_Equal()
    {
        var some = NullableOption.Some(5);
        NullableOption<int> none = default;

        await Assert.That(some).IsNotEqualTo(none);
    }

    [Test]
    public async Task Equality_Two_Somes_With_Same_Value_Should_Be_Equal()
    {
        var a = NullableOption.Some(5);
        var b = NullableOption.Some(5);

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Equality_Two_Somes_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = NullableOption.Some(5);
        var b = NullableOption.Some(6);

        await Assert.That(a).IsNotEqualTo(b);
    }

    [Test]
    public async Task Equality_Some_Null_Should_Not_Equal_None()
    {
        var someNull = NullableOption.Some<string?>(null);
        NullableOption<string?> none = default;

        await Assert.That(someNull).IsNotEqualTo(none);
    }

    [Test]
    public async Task Equality_Two_Some_Nulls_Should_Be_Equal()
    {
        var a = NullableOption.Some<string?>(null);
        var b = NullableOption.Some<string?>(null);

        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task Implicit_Conversion_From_Value_Should_Create_Some()
    {
        NullableOption<int> option = 42;

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsEqualTo(42);
    }
    
    [Test]
    public async Task Implicit_Conversion_From_Null_Should_Create_Some_Null()
    {
        NullableOption<string?> option = null;

        await Assert.That(option.HasValue).IsTrue();
        await Assert.That(option.Value).IsNull();
    }

    [Test]
    public async Task Implicit_Conversion_From_Option_Should_Work()
    {
        Option<int> opt = Option.Some(42);
        NullableOption<int> nOpt = opt;

        await Assert.That(nOpt.HasValue).IsTrue();
        await Assert.That(nOpt.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Implicit_Conversion_From_Option_None_Should_Work()
    {
        Option<int> opt = Option.None<int>();
        NullableOption<int> nOpt = opt;

        await Assert.That(nOpt.HasValue).IsFalse();
    }

    [Test]
    public async Task Explicit_Conversion_To_Option_Should_Work()
    {
        NullableOption<int> nOpt = NullableOption.Some(42);
        Option<int> opt = (Option<int>)nOpt;

        await Assert.That(opt.HasValue).IsTrue();
        await Assert.That(opt.Value).IsEqualTo(42);
    }

    [Test]
    public async Task Explicit_Conversion_To_Option_From_None_Should_Work()
    {
        NullableOption<int> nOpt = NullableOption.None<int>();
        Option<int> opt = (Option<int>)nOpt;

        await Assert.That(opt.HasValue).IsFalse();
    }

    [Test]
    public async Task Explicit_Conversion_To_Option_From_Null_Should_Throw()
    {
        NullableOption<string?> nOpt = NullableOption.Some<string?>(null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
        {
            var _ = (Option<string?>)nOpt;
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Explicit_Conversion_To_Value_Should_Return_Value()
    {
        var option = NullableOption.Some(42);
        int? value = (int?)option;

        await Assert.That(value).IsEqualTo(42);
    }

    [Test]
    public async Task Explicit_Conversion_From_None_Should_Throw()
    {
        NullableOption<int> none = default;

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
        {
            var _ = (int?)none;
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task Or_Value_Should_Return_Content_Or_Fallback()
    {
        await Assert.That(NullableOption.Some(5).Or(10)).IsEqualTo(5);
        await Assert.That(NullableOption.None<int>().Or(10)).IsEqualTo(10);
    }

    [Test]
    public async Task Or_Factory_Should_Return_Content_Or_Invoke_Factory()
    {
        await Assert.That(NullableOption.Some(5).Or(() => 10)).IsEqualTo(5);
        await Assert.That(NullableOption.None<int>().Or(() => 10)).IsEqualTo(10);
    }

    [Test]
    public async Task OrDefault_Should_Return_Content_Or_Type_Default()
    {
        await Assert.That(NullableOption.Some(5).OrDefault()).IsEqualTo(5);
        await Assert.That(NullableOption.None<int>().OrDefault()).IsEqualTo(0);
    }

    [Test]
    public async Task OrThrow_Should_Return_Value_Or_Throw()
    {
        await Assert.That(NullableOption.Some(5).OrThrow()).IsEqualTo(5);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
        {
             NullableOption.None<int>().OrThrow();
             await Task.CompletedTask;
        });
    }

    [Test]
    public async Task Deconstruct_Should_Support_Pattern_Matching()
    {
        var some = NullableOption.Some(42);
        if (some is (true, var val))
        {
            await Assert.That(val).IsEqualTo(42);
        }
        else
        {
            Assert.Fail("Should have matched Some pattern");
        }

        var none = NullableOption.None<int>();
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
