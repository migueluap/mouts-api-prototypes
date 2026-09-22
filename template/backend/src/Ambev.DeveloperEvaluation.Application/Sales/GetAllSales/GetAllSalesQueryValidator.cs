using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

/// <summary>
/// Validator for GetAllSalesQuery.
/// </summary>
public class GetAllSalesQueryValidator : AbstractValidator<GetAllSalesQuery>
{
    /// <summary>
    /// Initializes validation rules for GetAllSalesQuery.
    /// </summary>
    public GetAllSalesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .WithMessage("Size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Size must not exceed 100 items per page.");

        RuleFor(x => x.MinDate)
            .LessThanOrEqualTo(x => x.MaxDate)
            .When(x => x.MinDate.HasValue && x.MaxDate.HasValue)
            .WithMessage("MinDate must be less than or equal to MaxDate.");

        RuleFor(x => x.MinTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinTotalAmount.HasValue)
            .WithMessage("MinTotalAmount must be greater than or equal to 0.");

        RuleFor(x => x.MaxTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxTotalAmount.HasValue)
            .WithMessage("MaxTotalAmount must be greater than or equal to 0.");

        RuleFor(x => x.MinTotalAmount)
            .LessThanOrEqualTo(x => x.MaxTotalAmount)
            .When(x => x.MinTotalAmount.HasValue && x.MaxTotalAmount.HasValue)
            .WithMessage("MinTotalAmount must be less than or equal to MaxTotalAmount.");

        RuleFor(x => x.Order)
            .Must(BeValidOrderFormat)
            .When(x => !string.IsNullOrWhiteSpace(x.Order))
            .WithMessage("Order format is invalid. Expected format: 'field1 desc, field2 asc' or 'field1 desc, field2'.");
    }

    /// <summary>
    /// Validates the order format.
    /// </summary>
    /// <param name="order">The order string</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool BeValidOrderFormat(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return true;

        var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var validFields = new[]
        {
            "id", "salenumber", "customername", "branchname",
            "date", "totalamount", "cancelled", "itemcount"
        };

        foreach (var part in parts)
        {
            var trimmed = part.Trim().ToLowerInvariant();
            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length == 0 || tokens.Length > 2)
                return false;

            // Validate field name
            if (!validFields.Contains(tokens[0]))
                return false;

            // Validate direction if present
            if (tokens.Length == 2 && tokens[1] != "asc" && tokens[1] != "desc")
                return false;
        }

        return true;
    }
}
