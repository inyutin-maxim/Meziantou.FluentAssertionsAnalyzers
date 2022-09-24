using System.Globalization;
using Meziantou.FluentAssertionsAnalyzers.Tests.Helpers;
using TestHelper;
using Xunit;

namespace Meziantou.FluentAssertionsAnalyzers.Tests;

public class NullableValueSimplificationAnalyzerUnitTests
{
    [Theory]
    [InlineData(typeof(bool))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(TimeSpan))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateOnly))]
    [InlineData(typeof(TimeOnly))]
    [InlineData(typeof(NumberStyles))]
    public Task NullableValueHasValueSimplificationWhenShouldBeTrueTests(Type type)
    {
        return CreateProjectBuilder()
            .WithSourceCode($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        {{ type.FullName}} ? subject = default;
        [|subject.HasValue.Should().BeTrue()|];
    }
}
""" )
            .ShouldFixCodeWith($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        {{ type.FullName}} ? subject = default;
        subject.Should().HaveValue();
    }
}
""" )
            .ValidateAsync();
    }

    [Theory]
    [InlineData(typeof(bool))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(TimeSpan))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateOnly))]
    [InlineData(typeof(TimeOnly))]
    [InlineData(typeof(NumberStyles))]
    public Task NullableValueHasValueSimplificationWhenShouldBeFalseTests(Type type)
    {
        return CreateProjectBuilder()
            .WithSourceCode($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        {{ type.FullName}} ? subject = default;
        [|subject.HasValue.Should().BeFalse()|];
    }
}
""" )
            .ShouldFixCodeWith($$"""
using FluentAssertions;

class Test
{
    public void MyTest()
    {
        {{ type.FullName}} ? subject = default;
        subject.Should().NotHaveValue();
    }
}
""" )
            .ValidateAsync();
    }

    private static ProjectBuilder CreateProjectBuilder()
    {
        return new ProjectBuilder()
            .WithTargetFramework(TargetFramework.Net6_0)
            .WithAnalyzer<NullableValueSimplificationAnalyzer>()
            .AddAllCodeFixers()
            .AddFluentAssertionsApi();
    }
}