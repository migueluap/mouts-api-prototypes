using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
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
/// Contains unit tests for the <see cref="CancelSaleHandler"/> class.
/// </summary>
public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CancelSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelSaleHandlerTests"/> class.
    /// Sets up the test dependencies and handler.
    /// </summary>
    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CancelSaleHandler(_saleRepository, _mapper, _publisher);
    }

    /// <summary>
    /// Tests that a valid cancel sale request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When cancelling sale Then returns cancelled sale")]
    public async Task Handle_ValidSale_ReturnsCancelledSale()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var command = new CancelSaleCommand(sale.Id);

        var result = new CancelSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            Cancelled = true,
            CancelledAt = DateTime.UtcNow
        };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(result);

        // When
        var cancelSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        cancelSaleResult.Should().NotBeNull();
        cancelSaleResult.Id.Should().Be(sale.Id);
        cancelSaleResult.Cancelled.Should().BeTrue();
        sale.Cancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that cancelling a non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale ID When cancelling sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSaleId_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new CancelSaleCommand(Guid.NewGuid());

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.SaleId} not found");
    }

    /// <summary>
    /// Tests that cancelling an already cancelled sale throws InvalidOperationException.
    /// </summary>
    [Fact(DisplayName = "Given already cancelled sale When cancelling sale Then throws InvalidOperationException")]
    public async Task Handle_AlreadyCancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        sale.Cancel(); // Cancel it first
        var command = new CancelSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Sale is already cancelled");
    }

    /// <summary>
    /// Tests that the repository is called to update the cancelled sale.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When cancelling Then calls repository to update sale")]
    public async Task Handle_ValidSale_CallsRepositoryUpdate()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var command = new CancelSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(new CancelSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(s => s.Id == sale.Id && s.Cancelled),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the mapper is called to convert the cancelled sale to result.
    /// </summary>
    [Fact(DisplayName = "Given cancelled sale When handling Then maps sale to result")]
    public async Task Handle_ValidSale_MapsSaleToResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var command = new CancelSaleCommand(sale.Id);
        var result = new CancelSaleResult { Id = sale.Id, Cancelled = true };

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<CancelSaleResult>(Arg.Is<Sale>(s => s.Cancelled));
    }

    /// <summary>
    /// Tests that all items are also marked as cancelled when sale is cancelled.
    /// </summary>
    [Fact(DisplayName = "Given sale with items When cancelling sale Then all items are cancelled")]
    public async Task Handle_SaleWithItems_CancelsAllItems()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(3);
        var command = new CancelSaleCommand(sale.Id);

        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(new CancelSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        sale.Items.Should().OnlyContain(item => item.Cancelled);
        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(s => s.Items.All(i => i.Cancelled)),
            Arg.Any<CancellationToken>());
    }
}
