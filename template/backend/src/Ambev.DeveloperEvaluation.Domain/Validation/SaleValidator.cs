using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

/// <summary>
/// Validator for Sale entity that enforces business rules and data integrity.
/// </summary>
public class SaleValidator : AbstractValidator<Sale>
{
    /// <summary>
    /// Initializes a new instance of SaleValidator with validation rules.
    /// </summary>
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty()
            .WithMessage("Sale number is required")
            .MaximumLength(50)
            .WithMessage("Sale number cannot exceed 50 characters");

        RuleFor(sale => sale.Date)
            .NotEmpty()
            .WithMessage("Sale date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Sale date cannot be in the future");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty()
            .WithMessage("Customer name is required")
            .MaximumLength(200)
            .WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(sale => sale.BranchId)
            .NotEmpty()
            .WithMessage("Branch ID is required");

        RuleFor(sale => sale.BranchName)
            .NotEmpty()
            .WithMessage("Branch name is required")
            .MaximumLength(200)
            .WithMessage("Branch name cannot exceed 200 characters");

        RuleFor(sale => sale.Items)
            .NotEmpty()
            .WithMessage("Sale must contain at least one item")
            .When(sale => !sale.Cancelled);

        RuleFor(sale => sale.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount cannot be negative");

        // Validate that total amount matches sum of items
        RuleFor(sale => sale)
            .Must(sale => ValidateTotalAmount(sale))
            .WithMessage(sale =>
                $"Total amount mismatch. Calculated: {CalculateExpectedTotal(sale):C}, " +
                $"Stored: {sale.TotalAmount:C}")
            .When(sale => sale.Items.Any());

        // Validate each item in the collection
        RuleForEach(sale => sale.Items)
            .SetValidator(new SaleItemValidator());
    }

    /// <summary>
    /// Validates that the stored total amount matches the sum of all non-cancelled items.
    /// </summary>
    private bool ValidateTotalAmount(Sale sale)
    {
        var expectedTotal = CalculateExpectedTotal(sale);
        return Math.Abs(sale.TotalAmount - expectedTotal) < 0.01m; // Allow small floating point differences
    }

    /// <summary>
    /// Calculates the expected total amount by summing all non-cancelled items.
    /// </summary>
    private decimal CalculateExpectedTotal(Sale sale)
    {
        return sale.Items
            .Where(item => !item.Cancelled)
            .Sum(item => item.TotalAmount);
    }
}
