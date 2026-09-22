using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for SaleItem entity that enforces business rules and data integrity.
/// </summary>
public class SaleItemValidator : AbstractValidator<SaleItem>
{
    /// <summary>
    /// Initializes a new instance of SaleItemValidator with validation rules.
    /// </summary>
    public SaleItemValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(item => item.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(item => item.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(20)
            .WithMessage("Quantity cannot exceed 20 items (business rule)");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than 0");

        RuleFor(item => item.Discount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount cannot be negative")
            .LessThanOrEqualTo(100)
            .WithMessage("Discount cannot exceed 100%");

        // Validate discount matches quantity-based business rules
        RuleFor(item => item)
            .Must(item => ValidateDiscountRule(item))
            .WithMessage(item => $"Invalid discount for quantity {item.Quantity}. " +
                               $"Expected: {GetExpectedDiscount(item.Quantity)}%, Got: {item.Discount}%")
            .When(item => item.Discount > 0 || item.Quantity >= 4);

        RuleFor(item => item.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount cannot be negative");
    }

    /// <summary>
    /// Validates that the discount applied matches the quantity-based business rules.
    /// </summary>
    private bool ValidateDiscountRule(SaleItem item)
    {
        var expectedDiscount = GetExpectedDiscount(item.Quantity);
        return Math.Abs(item.Discount - expectedDiscount) < 0.01m; // Allow small floating point differences
    }

    /// <summary>
    /// Gets the expected discount percentage based on quantity.
    /// Business Rules:
    /// - Less than 4 items: 0% discount
    /// - 4 to 9 items: 10% discount
    /// - 10 to 20 items: 20% discount
    /// </summary>
    private decimal GetExpectedDiscount(int quantity)
    {
        if (quantity < 4) return 0;
        if (quantity >= 4 && quantity <= 9) return 10;
        if (quantity >= 10 && quantity <= 20) return 20;
        return 0;
    }
}
