using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.Sales;

/// <summary>
/// Contains unit tests for the <see cref="SaleItem"/> domain entity.
/// Tests the business logic and domain rules of the SaleItem entity.
/// </summary>
public class SaleItemTests
{
    /// <summary>
    /// Tests that Create factory method successfully creates a valid sale item.
    /// </summary>
    [Fact(DisplayName = "Given valid parameters When creating sale item Then returns valid item")]
    public void Create_ValidParameters_ReturnsValidItem()
    {
        // Given
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 5;
        var unitPrice = 100m;

        // When
        var item = SaleItem.Create(productId, productName, quantity, unitPrice);

        // Then
        item.Should().NotBeNull();
        item.Id.Should().Be(Guid.Empty); // Id is not set automatically
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(unitPrice);
        item.Discount.Should().Be(10); // 5 items = 10% discount
        item.TotalAmount.Should().Be(450m); // 5 * 100 * 0.9
        item.Cancelled.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Create throws exception when quantity is less than 1.
    /// </summary>
    [Theory(DisplayName = "Given invalid quantity When creating sale item Then throws ArgumentException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(100)]
    public void Create_InvalidQuantity_ThrowsArgumentException(int invalidQuantity)
    {
        // Given
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var unitPrice = 100m;

        // When
        var act = () => SaleItem.Create(productId, productName, invalidQuantity, unitPrice);

        // Then
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be between 1 and 20*");
    }

    /// <summary>
    /// Tests that Create throws exception when unit price is zero or negative.
    /// </summary>
    [Theory(DisplayName = "Given invalid unit price When creating sale item Then throws ArgumentException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_InvalidUnitPrice_ThrowsArgumentException(decimal invalidPrice)
    {
        // Given
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 5;

        // When
        var act = () => SaleItem.Create(productId, productName, quantity, invalidPrice);

        // Then
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price must be greater than 0*");
    }

    /// <summary>
    /// Tests that CalculateDiscount returns 0% for quantities less than 4.
    /// </summary>
    [Theory(DisplayName = "Given quantity less than 4 When calculating discount Then returns 0 percent")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CalculateDiscount_QuantityLessThan4_Returns0Percent(int quantity)
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", quantity, 100m);

        // When
        var discount = item.CalculateDiscount();

        // Then
        discount.Should().Be(0);
        item.Discount.Should().Be(0);
    }

    /// <summary>
    /// Tests that CalculateDiscount returns 10% for quantities between 4 and 9.
    /// </summary>
    [Theory(DisplayName = "Given quantity 4 to 9 When calculating discount Then returns 10 percent")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    public void CalculateDiscount_Quantity4To9_Returns10Percent(int quantity)
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", quantity, 100m);

        // When
        var discount = item.CalculateDiscount();

        // Then
        discount.Should().Be(10);
        item.Discount.Should().Be(10);
    }

    /// <summary>
    /// Tests that CalculateDiscount returns 20% for quantities between 10 and 20.
    /// </summary>
    [Theory(DisplayName = "Given quantity 10 to 20 When calculating discount Then returns 20 percent")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(18)]
    [InlineData(20)]
    public void CalculateDiscount_Quantity10To20_Returns20Percent(int quantity)
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", quantity, 100m);

        // When
        var discount = item.CalculateDiscount();

        // Then
        discount.Should().Be(20);
        item.Discount.Should().Be(20);
    }

    /// <summary>
    /// Tests that TotalAmount is calculated correctly with no discount.
    /// </summary>
    [Fact(DisplayName = "Given item with 3 units When calculating total Then applies no discount")]
    public void TotalAmount_Quantity3_NoDiscount()
    {
        // Given & When
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 3, 100m);

        // Then
        item.TotalAmount.Should().Be(300m); // 3 * 100 * 1.0
    }

    /// <summary>
    /// Tests that TotalAmount is calculated correctly with 10% discount.
    /// </summary>
    [Fact(DisplayName = "Given item with 5 units When calculating total Then applies 10 percent discount")]
    public void TotalAmount_Quantity5_Applies10PercentDiscount()
    {
        // Given & When
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 100m);

        // Then
        item.TotalAmount.Should().Be(450m); // 5 * 100 * 0.9
    }

    /// <summary>
    /// Tests that TotalAmount is calculated correctly with 20% discount.
    /// </summary>
    [Fact(DisplayName = "Given item with 10 units When calculating total Then applies 20 percent discount")]
    public void TotalAmount_Quantity10_Applies20PercentDiscount()
    {
        // Given & When
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 10, 100m);

        // Then
        item.TotalAmount.Should().Be(800m); // 10 * 100 * 0.8
    }

    /// <summary>
    /// Tests that UpdateQuantity successfully updates quantity and recalculates totals.
    /// </summary>
    [Fact(DisplayName = "Given valid new quantity When updating quantity Then updates and recalculates")]
    public void UpdateQuantity_ValidQuantity_UpdatesAndRecalculates()
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 3, 100m);
        var initialTotal = item.TotalAmount;

        // When
        item.UpdateQuantity(10);

        // Then
        item.Quantity.Should().Be(10);
        item.Discount.Should().Be(20); // 10 items = 20% discount
        item.TotalAmount.Should().Be(800m); // 10 * 100 * 0.8
        item.TotalAmount.Should().NotBe(initialTotal);
    }

    /// <summary>
    /// Tests that UpdateQuantity throws exception for invalid quantities.
    /// </summary>
    [Theory(DisplayName = "Given invalid quantity When updating quantity Then throws ArgumentException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(100)]
    public void UpdateQuantity_InvalidQuantity_ThrowsArgumentException(int invalidQuantity)
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 100m);

        // When
        var act = () => item.UpdateQuantity(invalidQuantity);

        // Then
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be between 1 and 20*");
    }

    /// <summary>
    /// Tests that UpdateQuantity throws exception when item is cancelled.
    /// </summary>
    [Fact(DisplayName = "Given cancelled item When updating quantity Then throws InvalidOperationException")]
    public void UpdateQuantity_CancelledItem_ThrowsInvalidOperationException()
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 100m);
        item.Cancel();

        // When
        var act = () => item.UpdateQuantity(10);

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update quantity of a cancelled item");
    }

    /// <summary>
    /// Tests that Cancel successfully cancels the item.
    /// </summary>
    [Fact(DisplayName = "Given valid item When cancelling Then item is cancelled")]
    public void Cancel_ValidItem_CancelsItem()
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 100m);

        // When
        item.Cancel();

        // Then
        item.Cancelled.Should().BeTrue();
        item.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that Cancel throws exception when item is already cancelled.
    /// </summary>
    [Fact(DisplayName = "Given already cancelled item When cancelling again Then throws InvalidOperationException")]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 100m);
        item.Cancel();

        // When
        var act = () => item.Cancel();

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Item is already cancelled");
    }

    /// <summary>
    /// Tests that discount calculation changes correctly when quantity is updated.
    /// </summary>
    [Fact(DisplayName = "Given item When updating quantity across discount thresholds Then discount updates correctly")]
    public void UpdateQuantity_AcrossDiscountThresholds_UpdatesDiscountCorrectly()
    {
        // Given
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 2, 100m);

        // When & Then - Start with no discount
        item.Discount.Should().Be(0);
        item.TotalAmount.Should().Be(200m);

        // Update to 5 items - should get 10% discount
        item.UpdateQuantity(5);
        item.Discount.Should().Be(10);
        item.TotalAmount.Should().Be(450m);

        // Update to 12 items - should get 20% discount
        item.UpdateQuantity(12);
        item.Discount.Should().Be(20);
        item.TotalAmount.Should().Be(960m);

        // Update back to 3 items - should get no discount
        item.UpdateQuantity(3);
        item.Discount.Should().Be(0);
        item.TotalAmount.Should().Be(300m);
    }

    /// <summary>
    /// Tests that TotalAmount calculation is precise with decimal values.
    /// </summary>
    [Fact(DisplayName = "Given item with decimal unit price When calculating total Then calculation is precise")]
    public void TotalAmount_DecimalUnitPrice_CalculationIsPrecise()
    {
        // Given & When
        var item = SaleItem.Create(Guid.NewGuid(), "Test Product", 5, 99.99m);

        // Then
        item.TotalAmount.Should().Be(449.955m); // 5 * 99.99 * 0.9
    }
}
