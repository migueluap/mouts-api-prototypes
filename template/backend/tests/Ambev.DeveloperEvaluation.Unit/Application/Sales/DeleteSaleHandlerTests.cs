using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Contains unit tests for the <see cref="DeleteSaleHandler"/> class.
/// </summary>
public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly DeleteSaleHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSaleHandlerTests"/> class.
    /// Sets up the test dependencies and handler.
    /// </summary>
    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    /// <summary>
    /// Tests that a valid delete sale request is handled successfully.
    /// </summary>
    [Fact(DisplayName = "Given valid sale ID When deleting sale Then deletes successfully")]
    public async Task Handle_ValidSaleId_DeletesSuccessfully()
    {
        // Given
        var command = new DeleteSaleCommand(Guid.NewGuid());

        _saleRepository.DeleteAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(true);

        // When
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
        await _saleRepository.Received(1).DeleteAsync(command.SaleId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that deleting a non-existent sale throws KeyNotFoundException.
    /// </summary>
    [Fact(DisplayName = "Given non-existent sale ID When deleting sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistentSaleId_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new DeleteSaleCommand(Guid.NewGuid());

        _saleRepository.DeleteAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(false);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.SaleId} not found");
    }

    /// <summary>
    /// Tests that the repository is called with the correct sale ID.
    /// </summary>
    [Fact(DisplayName = "Given sale ID When deleting Then calls repository with correct ID")]
    public async Task Handle_ValidCommand_CallsRepositoryWithCorrectId()
    {
        // Given
        var command = new DeleteSaleCommand(Guid.NewGuid());

        _saleRepository.DeleteAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(true);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).DeleteAsync(
            Arg.Is<Guid>(id => id == command.SaleId),
            Arg.Any<CancellationToken>());
    }
}
