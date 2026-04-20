using BankingOperationsService.Domain.Common;
using NUnit.Framework;

namespace BankingOperationsService.Domain.UnitTests.Common;

[TestFixture]
public class ValueObjectTests
{
    private class Money : ValueObject
    {
        private readonly decimal _amount;
        private readonly string _currency;

        public Money(decimal amount, string currency)
        {
            _amount = amount;
            _currency = currency;
        }
    }

    [Test]
    public void Equals_WithSameFieldValues_ShouldReturnTrue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_WithDifferentAmount_ShouldReturnFalse()
    {
        var a = new Money(10m, "USD");
        var b = new Money(20m, "USD");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_WithDifferentCurrency_ShouldReturnFalse()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "EUR");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void GetHashCode_WithSameFieldValues_ShouldReturnSameHash()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void EqualityOperator_WithSameFieldValues_ShouldReturnTrue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.That(a == b, Is.True);
    }

    [Test]
    public void InequalityOperator_WithDifferentFieldValues_ShouldReturnTrue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(99m, "GBP");

        Assert.That(a != b, Is.True);
    }
}
