using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Validator for CreateSaleCommand that enforces business rules for sale creation.
/// </summary>
public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    /// <summary>
    /// Initializes validation rules for CreateSaleCommand.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - SaleNumber: Required, max length 50 characters
    /// - CustomerId: Must not be empty GUID
    /// - CustomerName: Required, max length 200 characters
    /// - BranchId: Must not be empty GUID
    /// - BranchName: Required, max length 200 characters
    /// - Date: Must not be default/empty
    /// - Items: Must have at least one item, each validated by CreateSaleItemCommandValidator
    /// </remarks>
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.SaleNumber)
            .NotEmpty().WithMessage("Sale number is required")
            .MaximumLength(50).WithMessage("Sale number cannot exceed 50 characters");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(x => x.BranchName)
            .NotEmpty().WithMessage("Branch name is required")
            .MaximumLength(200).WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Sale date is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sale must have at least one item")
            .Must(items => items != null && items.Count > 0).WithMessage("Sale must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateSaleItemCommandValidator());
    }
}

/// <summary>
/// Validator for CreateSaleItemCommand that enforces business rules for sale items.
/// </summary>
public class CreateSaleItemCommandValidator : AbstractValidator<CreateSaleItemCommand>
{
    /// <summary>
    /// Initializes validation rules for CreateSaleItemCommand.
    /// </summary>
    /// <remarks>
    /// Validation rules include:
    /// - ProductId: Must not be empty GUID
    /// - ProductName: Required, max length 200 characters
    /// - Quantity: Must be between 1 and 20 (business rule)
    /// - UnitPrice: Must be greater than 0
    /// - Discount: Must be between 0 and 100
    /// </remarks>
    public CreateSaleItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 20)
            .WithMessage("Quantity must be between 1 and 20");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x.Discount)
            .InclusiveBetween(0, 100)
            .WithMessage("Discount must be between 0 and 100");
    }
}
