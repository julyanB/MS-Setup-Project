using BankingOperationsService.Domain.Common;
using BankingOperationsService.Domain.Exceptions;
using NUnit.Framework;

namespace BankingOperationsService.Domain.UnitTests.Common;

[TestFixture]
public class GuardTests
{
    private class TestException : BaseDomainException { }

    [Test]
    public void AgainstEmptyString_WithNonEmptyValue_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() =>
            Guard.AgainstEmptyString<TestException>("valid", "Field"));
    }

    [TestCase("")]
    [TestCase(null)]
    public void AgainstEmptyString_WithEmptyOrNull_ShouldThrow(string value)
    {
        Assert.Throws<TestException>(() =>
            Guard.AgainstEmptyString<TestException>(value, "Field"));
    }

    [TestCase("ab", 2, 10)]
    [TestCase("abcde", 2, 10)]
    [TestCase("abcdefghij", 2, 10)]
    public void ForStringLength_WithinBounds_ShouldNotThrow(string value, int min, int max)
    {
        Assert.DoesNotThrow(() =>
            Guard.ForStringLength<TestException>(value, min, max, "Field"));
    }

    [TestCase("a", 2, 10)]
    [TestCase("abcdefghijk", 2, 10)]
    public void ForStringLength_OutOfBounds_ShouldThrow(string value, int min, int max)
    {
        Assert.Throws<TestException>(() =>
            Guard.ForStringLength<TestException>(value, min, max, "Field"));
    }

    [TestCase(5, 1, 10)]
    [TestCase(1, 1, 10)]
    [TestCase(10, 1, 10)]
    public void AgainstOutOfRange_Int_WithinBounds_ShouldNotThrow(int value, int min, int max)
    {
        Assert.DoesNotThrow(() =>
            Guard.AgainstOutOfRange<TestException>(value, min, max, "Field"));
    }

    [TestCase(0, 1, 10)]
    [TestCase(11, 1, 10)]
    public void AgainstOutOfRange_Int_OutOfBounds_ShouldThrow(int value, int min, int max)
    {
        Assert.Throws<TestException>(() =>
            Guard.AgainstOutOfRange<TestException>(value, min, max, "Field"));
    }

    [Test]
    public void ForValidUrl_WithValidUrl_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() =>
            Guard.ForValidUrl<TestException>("https://example.com", "Url"));
    }

    [TestCase("not-a-url")]
    [TestCase("ftp//missing-colon")]
    public void ForValidUrl_WithInvalidUrl_ShouldThrow(string url)
    {
        Assert.Throws<TestException>(() =>
            Guard.ForValidUrl<TestException>(url, "Url"));
    }

    [Test]
    public void Against_WhenValueDiffersFromUnexpected_ShouldNotThrow()
    {
        Assert.DoesNotThrow(() =>
            Guard.Against<TestException>("actual", "unexpected", "Field"));
    }

    [Test]
    public void Against_WhenValueEqualsUnexpected_ShouldThrow()
    {
        Assert.Throws<TestException>(() =>
            Guard.Against<TestException>("same", "same", "Field"));
    }
}
