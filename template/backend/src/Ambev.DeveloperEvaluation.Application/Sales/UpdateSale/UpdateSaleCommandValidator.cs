using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Validator for UpdateSaleCommand that enforces business rules for sale updates.
/// </summary>
public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    /// <summary>
    /// Initializes validation rules for UpdateSaleCommand.
    /// </summary>
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty().WithMessage("Sale ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sale must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new UpdateSaleItemCommandValidator());
    }
}

/// <summary>
/// Validator for UpdateSaleItemCommand.
/// </summary>
public class UpdateSaleItemCommandValidator : AbstractValidator<UpdateSaleItemCommand>
{
    /// <summary>
    /// Initializes validation rules for UpdateSaleItemCommand.
    /// </summary>
    public UpdateSaleItemCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 20)
            .WithMessage("Quantity must be between 1 and 20");

        RuleFor(x => x.Discount)
            .InclusiveBetween(0, 100)
            .WithMessage("Discount must be between 0 and 100");

        // When adding a new item (ItemId is empty), ProductId and other fields are required
        When(x => x.ItemId == Guid.Empty, () =>
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required for new items");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required for new items")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0");
        });
    }
}
