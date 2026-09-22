using Ambev.DeveloperEvaluation.Common.Validation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Command for updating an existing sale.
/// </summary>
/// <remarks>
/// This command allows updating sale item quantities.
/// Note: Customer, Branch, and Date cannot be changed after creation for audit purposes.
/// </remarks>
public class UpdateSaleCommand : IRequest<UpdateSaleResult>
{
    /// <summary>
    /// Gets or sets the ID of the sale to update.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the list of items to update.
    /// Each item must include the ItemId for updates.
    /// </summary>
    public List<UpdateSaleItemCommand> Items { get; set; } = new();

    /// <summary>
    /// Validates the command using FluentValidation.
    /// </summary>
    /// <returns>A validation result indicating whether the command is valid</returns>
    public ValidationResultDetail Validate()
    {
        var validator = new UpdateSaleCommandValidator();
        var result = validator.Validate(this);
        return new ValidationResultDetail
        {
            IsValid = result.IsValid,
            Errors = result.Errors.Select(o => (ValidationErrorDetail)o)
        };
    }
}
