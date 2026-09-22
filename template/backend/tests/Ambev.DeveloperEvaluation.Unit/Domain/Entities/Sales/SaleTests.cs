using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.Sales;

/// <summary>
/// Contains unit tests for the <see cref="Sale"/> domain entity.
/// Tests the business logic and domain rules of the Sale aggregate root.
/// </summary>
public class SaleTests
{
    /// <summary>
    /// Tests that a new sale is created with correct default values.
    /// </summary>
    [Fact(DisplayName = "Given new sale When created Then has correct default values")]
    public void Constructor_NewSale_HasCorrectDefaults()
    {
        // Given & When
        var sale = new Sale();

        // Then
        sale.Id.Should().Be(Guid.Empty); // Id is not set automatically
        sale.Cancelled.Should().BeFalse();
        sale.CancelledAt.Should().BeNull();
        sale.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.Items.Should().BeEmpty();
        sale.TotalAmount.Should().Be(0);
    }

    /// <summary>
    /// Tests that AddItem successfully adds an item to the sale.
    /// </summary>
    [Fact(DisplayName = "Given valid item When adding item Then item is added and totals calculated")]
    public void AddItem_ValidItem_AddsItemAndCalculatesTotals()
    {
        // Given
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleItem.Create(Guid.NewGuid(), "Product 1", 5, 100m);

        // When
        sale.AddItem(item);

        // Then
        sale.Items.Should().HaveCount(1);
        sale.Items.Should().Contain(item);
        sale.TotalAmount.Should().Be(450m); // 5 * 100 * 0.9 (10% discount for 5 items)
        sale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that adding an item to a cancelled sale throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When adding item Then throws InvalidOperationException")]
    public void AddItem_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(1);
        sale.Cancel();
        var item = SaleItem.Create(Guid.NewGuid(), "Product 1", 5, 100m);

        // When
        var act = () => sale.AddItem(item);

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add items to a cancelled sale");
    }

    /// <summary>
    /// Tests that RemoveItem successfully removes an item from the sale.
    /// </summary>
    [Fact(DisplayName = "Given sale with items When removing item Then item is removed and totals recalculated")]
    public void RemoveItem_ValidItem_RemovesItemAndRecalculatesTotals()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemToRemove = sale.Items.First();
        var initialCount = sale.Items.Count;

        // When
        sale.RemoveItem(itemToRemove.Id);

        // Then
        sale.Items.Should().HaveCount(initialCount - 1);
        sale.Items.Should().NotContain(i => i.Id == itemToRemove.Id);
        sale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that removing a non-existent item throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given sale When removing non-existent item Then throws KeyNotFoundException")]
    public void RemoveItem_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var nonExistentId = Guid.NewGuid();

        // When
        var act = () => sale.RemoveItem(nonExistentId);

        // Then
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"Sale item with ID {nonExistentId} not found in this sale");
    }

    /// <summary>
    /// Tests that removing an item from a cancelled sale throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When removing item Then throws InvalidOperationException")]
    public void RemoveItem_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        // When
        var act = () => sale.RemoveItem(itemId);

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot remove items from a cancelled sale");
    }

    /// <summary>
    /// Tests that UpdateItemQuantity successfully updates an item's quantity.
    /// </summary>
    [Fact(DisplayName = "Given sale with items When updating item quantity Then quantity is updated and totals recalculated")]
    public void UpdateItemQuantity_ValidQuantity_UpdatesQuantityAndRecalculatesTotals()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemToUpdate = sale.Items.First();
        var newQuantity = 10;

        // When
        sale.UpdateItemQuantity(itemToUpdate.Id, newQuantity);

        // Then
        itemToUpdate.Quantity.Should().Be(newQuantity);
        itemToUpdate.Discount.Should().Be(20); // 10 items = 20% discount
        sale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that updating with invalid quantity throws ArgumentException.
    /// </summary>
    [Theory(DisplayName = "Given invalid quantity When updating item quantity Then throws ArgumentException")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(100)]
    public void UpdateItemQuantity_InvalidQuantity_ThrowsArgumentException(int invalidQuantity)
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemToUpdate = sale.Items.First();

        // When
        var act = () => sale.UpdateItemQuantity(itemToUpdate.Id, invalidQuantity);

        // Then
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be between 1 and 20*");
    }

    /// <summary>
    /// Tests that updating quantity for a non-existent item throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given sale When updating non-existent item quantity Then throws KeyNotFoundException")]
    public void UpdateItemQuantity_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var nonExistentId = Guid.NewGuid();

        // When
        var act = () => sale.UpdateItemQuantity(nonExistentId, 5);

        // Then
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"Sale item with ID {nonExistentId} not found in this sale");
    }

    /// <summary>
    /// Tests that updating item quantity in a cancelled sale throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When updating item quantity Then throws InvalidOperationException")]
    public void UpdateItemQuantity_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        // When
        var act = () => sale.UpdateItemQuantity(itemId, 5);

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update items in a cancelled sale");
    }

    /// <summary>
    /// Tests that CancelItem successfully cancels an individual item.
    /// </summary>
    [Fact(DisplayName = "Given sale with items When cancelling item Then item is cancelled and totals recalculated")]
    public void CancelItem_ValidItem_CancelsItemAndRecalculatesTotals()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemToCancel = sale.Items.First();
        var initialTotal = sale.TotalAmount;

        // When
        sale.CancelItem(itemToCancel.Id);

        // Then
        itemToCancel.Cancelled.Should().BeTrue();
        itemToCancel.CancelledAt.Should().NotBeNull();
        sale.TotalAmount.Should().BeLessThan(initialTotal);
        sale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that cancelling a non-existent item throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given sale When cancelling non-existent item Then throws KeyNotFoundException")]
    public void CancelItem_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var nonExistentId = Guid.NewGuid();

        // When
        var act = () => sale.CancelItem(nonExistentId);

        // Then
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"Sale item with ID {nonExistentId} not found in this sale");
    }

    /// <summary>
    /// Tests that cancelling an item in an already cancelled sale throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws InvalidOperationException")]
    public void CancelItem_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var itemId = sale.Items.First().Id;
        sale.Cancel();

        // When
        var act = () => sale.CancelItem(itemId);

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel items in an already cancelled sale");
    }

    /// <summary>
    /// Tests that Cancel successfully cancels the entire sale.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When cancelling sale Then sale is cancelled")]
    public void Cancel_ValidSale_CancelsSale()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);

        // When
        sale.Cancel();

        // Then
        sale.Cancelled.Should().BeTrue();
        sale.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sale.UpdatedAt.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that cancelling an already cancelled sale throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When cancelling again Then throws InvalidOperationException")]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel();

        // When
        var act = () => sale.Cancel();

        // Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Sale is already cancelled");
    }

    /// <summary>
    /// Tests that TotalAmount is calculated correctly from all non-cancelled items.
    /// </summary>
    [Fact(DisplayName = "Given sale with multiple items When calculating total Then sums all non-cancelled items")]
    public void TotalAmount_MultipleItems_SumsNonCancelledItems()
    {
        // Given
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };

        var item1 = SaleItem.Create(Guid.NewGuid(), "Product 1", 3, 100m); // 300 (no discount)
        var item2 = SaleItem.Create(Guid.NewGuid(), "Product 2", 5, 100m); // 450 (10% discount)
        var item3 = SaleItem.Create(Guid.NewGuid(), "Product 3", 10, 100m); // 800 (20% discount)

        // When
        sale.AddItem(item1);
        sale.AddItem(item2);
        sale.AddItem(item3);

        // Then
        sale.TotalAmount.Should().Be(1550m); // 300 + 450 + 800
    }

    /// <summary>
    /// Tests that TotalAmount excludes cancelled items.
    /// </summary>
    [Fact(DisplayName = "Given sale with cancelled item When calculating total Then excludes cancelled item")]
    public void TotalAmount_WithCancelledItem_ExcludesCancelledItem()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(3);
        var itemToCancel = sale.Items.First();

        // When
        sale.CancelItem(itemToCancel.Id);

        // Then
        sale.TotalAmount.Should().Be(sale.Items.Where(i => !i.Cancelled).Sum(i => i.TotalAmount));
    }

    /// <summary>
    /// Tests that Validate returns valid result for a valid sale.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When validating Then returns valid result")]
    public void Validate_ValidSale_ReturnsValidResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns invalid result for a sale with no items.
    /// </summary>
    [Fact(DisplayName = "Given sale with no items When validating Then returns invalid result")]
    public void Validate_SaleWithNoItems_ReturnsInvalidResult()
    {
        // Given
        var sale = new Sale
        {
            SaleNumber = "SALE-001",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests that Validate returns invalid result for a sale with empty sale number.
    /// </summary>
    [Fact(DisplayName = "Given sale with empty sale number When validating Then returns invalid result")]
    public void Validate_EmptySaleNumber_ReturnsInvalidResult()
    {
        // Given
        var sale = new Sale
        {
            SaleNumber = "",
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Test Branch"
        };
        var item = SaleItem.Create(Guid.NewGuid(), "Product 1", 5, 100m);
        sale.AddItem(item);

        // When
        var result = sale.Validate();

        // Then
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Detail.Contains("Sale number"));
    }
}
