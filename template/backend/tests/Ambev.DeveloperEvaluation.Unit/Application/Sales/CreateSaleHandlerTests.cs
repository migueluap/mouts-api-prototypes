using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
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
/// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly CreateSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSaleHandlerTests"/> class.
    /// Sets up the test dependencies and handler.
    /// </summary>
    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _publisher = Substitute.For<IPublisher>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _publisher);
    }

    /// <summary>
    /// Tests that a valid sale creation request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale(command.Items.Count);

        var result = new CreateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            Date = sale.Date,
            CustomerName = command.CustomerName,
            BranchName = command.BranchName,
            TotalAmount = sale.TotalAmount,
            Items = sale.Items.Select(i => new CreateSaleItemResult
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount
            }).ToList()
        };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);

        // When
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(sale.Id);
        createSaleResult.SaleNumber.Should().Be(sale.SaleNumber);
        createSaleResult.TotalAmount.Should().Be(sale.TotalAmount);
        createSaleResult.Items.Should().HaveCount(command.Items.Count);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that an invalid sale creation request throws a validation exception.
    /// </summary>
    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = SaleTestData.GenerateInvalidCreateSaleCommand();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that creating a sale with duplicate sale number throws an exception.
    /// </summary>
    [Fact(DisplayName = "Given duplicate sale number When creating sale Then throws InvalidOperationException")]
    public async Task Handle_DuplicateSaleNumber_ThrowsInvalidOperationException()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var existingSale = SaleTestData.GenerateValidSale(1);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(existingSale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Sale with number {command.SaleNumber} already exists");
    }

    /// <summary>
    /// Tests that the repository is called to check for duplicate sale numbers.
    /// </summary>
    [Fact(DisplayName = "Given valid command When handling Then checks for duplicate sale number")]
    public async Task Handle_ValidRequest_ChecksDuplicateSaleNumber()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale(command.Items.Count);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).GetBySaleNumberAsync(
            command.SaleNumber,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that all items from the command are added to the sale.
    /// </summary>
    [Fact(DisplayName = "Given command with items When creating sale Then adds all items to sale")]
    public async Task Handle_ValidRequest_AddsAllItems()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale(command.Items.Count);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.Items.Count == command.Items.Count),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the mapper is called to convert the created sale to result.
    /// </summary>
    [Fact(DisplayName = "Given valid command When handling Then maps sale to result")]
    public async Task Handle_ValidRequest_MapsSaleToResult()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale(command.Items.Count);
        var result = new CreateSaleResult { Id = sale.Id };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<CreateSaleResult>(Arg.Is<Sale>(s => s.Items.Count == command.Items.Count));
    }

    /// <summary>
    /// Tests that creating a sale with more than 20 identical items throws validation exception.
    /// </summary>
    [Fact(DisplayName = "Given sale with item quantity over 20 When creating sale Then throws validation exception")]
    public async Task Handle_ItemQuantityOver20_ThrowsValidationException()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        command.Items.First().Quantity = 21; // Exceeds maximum allowed quantity

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    /// <summary>
    /// Tests that the sale number is correctly set from the command.
    /// </summary>
    [Fact(DisplayName = "Given valid command When creating sale Then sets correct sale number")]
    public async Task Handle_ValidRequest_SetsCorrectSaleNumber()
    {
        // Given
        var command = SaleTestData.GenerateValidCreateSaleCommand();
        var sale = SaleTestData.GenerateValidSale(command.Items.Count);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleNumber == command.SaleNumber),
            Arg.Any<CancellationToken>());
    }
}
