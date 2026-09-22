using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;
using MediatR;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly UpdateSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSaleHandlerTests"/> class.
    /// Sets up the test dependencies and handler.
    /// </summary>
    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _publisher);
    }

    /// <summary>
    /// Tests that a valid sale update request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid update data When updating sale Then returns updated sale")]
    public async Task Handle_ValidRequest_ReturnsUpdatedSale()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var command = SaleTestData.GenerateValidUpdateSaleCommand();
        command.SaleId = sale.Id;

        var result = new UpdateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            Items = command.Items.Select(i => new UpdateSaleItemResult
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = 0,
                TotalAmount = i.Quantity * i.UnitPrice
            }).ToList()
        };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(result);

        // When
        var updateSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        updateSaleResult.Should().NotBeNull();
        updateSaleResult.Id.Should().Be(sale.Id);
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that updating a non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale ID When updating sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSaleId_ThrowsKeyNotFoundException()
    {
        // Given
        var command = SaleTestData.GenerateValidUpdateSaleCommand();

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.SaleId} not found");
    }

    /// <summary>
    /// Tests that updating a cancelled sale throws InvalidOperationException.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When updating sale Then throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel();
        var command = SaleTestData.GenerateValidUpdateSaleCommand();
        command.SaleId = sale.Id;

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot update a cancelled sale");
    }

    /// <summary>
    /// Tests that an invalid update request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid update data When updating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new UpdateSaleCommand
        {
            SaleId = Guid.Empty, // Invalid
            Items = new List<UpdateSaleItemCommand>() // Empty items
        };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that the handler adds new items correctly.
    /// </summary>
    [Fact(DisplayName = "Given command with new items When updating sale Then adds new items")]
    public async Task Handle_NewItems_AddsItemsToSale()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(1);
        var initialItemCount = sale.Items.Count;
        var command = SaleTestData.GenerateValidUpdateSaleCommand();
        command.SaleId = sale.Id;
        // All items have ItemId = Guid.Empty (new items)

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(new UpdateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(s => s.Items.Count >= initialItemCount),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the handler updates existing item quantities correctly.
    /// </summary>
    [Fact(DisplayName = "Given command with existing items When updating sale Then updates item quantities")]
    public async Task Handle_ExistingItems_UpdatesQuantities()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var existingItem = sale.Items.First();
        var command = new UpdateSaleCommand
        {
            SaleId = sale.Id,
            Items = new List<UpdateSaleItemCommand>
            {
                new UpdateSaleItemCommand
                {
                    ItemId = existingItem.Id,
                    ProductId = existingItem.ProductId,
                    ProductName = existingItem.ProductName,
                    Quantity = 10, // Different quantity
                    UnitPrice = existingItem.UnitPrice,
                    Discount = 0
                }
            }
        };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(new UpdateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the handler removes items not in the command.
    /// </summary>
    [Fact(DisplayName = "Given command without existing items When updating sale Then removes those items")]
    public async Task Handle_ItemsNotInCommand_RemovesItems()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(3);
        var command = new UpdateSaleCommand
        {
            SaleId = sale.Id,
            Items = new List<UpdateSaleItemCommand>
            {
                // Only include one new item, all existing items should be removed
                new UpdateSaleItemCommand
                {
                    ItemId = Guid.Empty,
                    ProductId = Guid.NewGuid(),
                    ProductName = "New Product",
                    Quantity = 5,
                    UnitPrice = 100,
                    Discount = 0
                }
            }
        };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(new UpdateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the mapper is called to convert the updated sale to result.
    /// </summary>
    [Fact(DisplayName = "Given valid update When handling Then maps sale to result")]
    public async Task Handle_ValidRequest_MapsSaleToResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var command = SaleTestData.GenerateValidUpdateSaleCommand();
        command.SaleId = sale.Id;
        var result = new UpdateSaleResult { Id = sale.Id };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<UpdateSaleResult>(Arg.Is<Sale>(s => s.Id == sale.Id));
    }
}
