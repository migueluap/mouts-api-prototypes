using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Contains unit tests for the <see cref="GetSaleHandler"/> class.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSaleHandlerTests"/> class.
    /// Sets up the test dependencies and handler.
    /// </summary>
    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    /// <summary>
    /// Tests that a valid get sale request returns the sale successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale ID When getting sale Then returns sale details")]
    public async Task Handle_ValidSaleId_ReturnsSaleDetails()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var query = new GetSaleQuery(sale.Id);

        var result = new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            Date = sale.Date,
            CustomerName = sale.CustomerName,
            BranchName = sale.BranchName,
            TotalAmount = sale.TotalAmount,
            Cancelled = sale.Cancelled,
            Items = sale.Items.Select(i => new GetSaleItemResult
            {
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount
            }).ToList()
        };

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // When
        var getSaleResult = await _handler.Handle(query, CancellationToken.None);

        // Then
        getSaleResult.Should().NotBeNull();
        getSaleResult.Id.Should().Be(sale.Id);
        getSaleResult.SaleNumber.Should().Be(sale.SaleNumber);
        getSaleResult.TotalAmount.Should().Be(sale.TotalAmount);
        getSaleResult.Items.Should().HaveCount(sale.Items.Count);
        await _saleRepository.Received(1).GetByIdAsync(query.Id, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that requesting a non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale ID When getting sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSaleId_ThrowsKeyNotFoundException()
    {
        // Given
        var query = new GetSaleQuery(Guid.NewGuid());

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {query.Id} not found");
    }

    /// <summary>
    /// Tests that the mapper is called with the correct sale entity.
    /// </summary>
    [Fact(DisplayName = "Given valid sale When handling Then maps sale to result")]
    public async Task Handle_ValidSale_MapsSaleToResult()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var query = new GetSaleQuery(sale.Id);
        var result = new GetSaleResult { Id = sale.Id };

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // When
        await _handler.Handle(query, CancellationToken.None);

        // Then
        _mapper.Received(1).Map<GetSaleResult>(Arg.Is<Sale>(s => s.Id == sale.Id));
    }

    /// <summary>
    /// Tests that the repository is called with the correct sale ID.
    /// </summary>
    [Fact(DisplayName = "Given sale ID When handling Then calls repository with correct ID")]
    public async Task Handle_ValidQuery_CallsRepositoryWithCorrectId()
    {
        // Given
        var sale = SaleTestData.GenerateValidSale(2);
        var query = new GetSaleQuery(sale.Id);

        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(new GetSaleResult());

        // When
        await _handler.Handle(query, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).GetByIdAsync(
            Arg.Is<Guid>(id => id == query.Id),
            Arg.Any<CancellationToken>());
    }
}
