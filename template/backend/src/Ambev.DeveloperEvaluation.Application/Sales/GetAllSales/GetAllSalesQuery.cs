using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

/// <summary>
/// Query for retrieving a paginated, filtered, and sorted list of sales.
/// </summary>
/// <remarks>
/// Supports pagination (_page, _size), ordering (_order), and filtering by various fields.
/// Implements the API conventions defined in .doc/general-api.md
/// </remarks>
public class GetAllSalesQuery : IRequest<GetAllSalesResult>
{
    /// <summary>
    /// Gets or sets the page number (default: 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page (default: 10).
    /// </summary>
    public int Size { get; set; } = 10;

    /// <summary>
    /// Gets or sets the ordering specification.
    /// Format: "field1 desc, field2 asc" or "field1 desc, field2"
    /// Default order is ascending if not specified.
    /// Example: "date desc, saleNumber asc"
    /// </summary>
    public string? Order { get; set; }

    /// <summary>
    /// Gets or sets the sale number filter (supports partial match with *).
    /// Example: "SAL-2024*" or "*001"
    /// </summary>
    public string? SaleNumber { get; set; }

    /// <summary>
    /// Gets or sets the customer name filter (supports partial match with *).
    /// Example: "John*" or "*Doe"
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the branch name filter (supports partial match with *).
    /// Example: "Branch*" or "*Central"
    /// </summary>
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the minimum date filter.
    /// Example: "2024-01-01"
    /// </summary>
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum date filter.
    /// Example: "2024-12-31"
    /// </summary>
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Gets or sets the minimum total amount filter.
    /// </summary>
    public decimal? MinTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum total amount filter.
    /// </summary>
    public decimal? MaxTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the cancelled status filter.
    /// If true, returns only cancelled sales.
    /// If false, returns only non-cancelled sales.
    /// If null, returns all sales.
    /// </summary>
    public bool? IsCancelled { get; set; }
}
