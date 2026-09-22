using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetAllSales;

/// <summary>
/// Request model for retrieving all sales with pagination, filtering, and ordering.
/// </summary>
/// <remarks>
/// Follows the API conventions defined in .doc/general-api.md
/// - Pagination: _page, _size
/// - Ordering: _order (e.g., "date desc, saleNumber asc")
/// - Filtering: field filters with wildcard support (*) and range filters (_min, _max)
/// </remarks>
public class GetAllSalesRequest
{
    /// <summary>
    /// Gets or sets the page number (1-based). Default is 1.
    /// </summary>
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size. Default is 10.
    /// </summary>
    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    /// <summary>
    /// Gets or sets the ordering specification.
    /// Format: "field1 desc, field2 asc" or "field1 desc, field2"
    /// Example: "date desc, saleNumber asc"
    /// </summary>
    [FromQuery(Name = "_order")]
    public string? Order { get; set; }

    /// <summary>
    /// Gets or sets the sale number filter (supports partial match with *).
    /// Example: "SAL-2024*" or "*001"
    /// </summary>
    [FromQuery(Name = "saleNumber")]
    public string? SaleNumber { get; set; }

    /// <summary>
    /// Gets or sets the customer name filter (supports partial match with *).
    /// Example: "John*" or "*Doe"
    /// </summary>
    [FromQuery(Name = "customerName")]
    public string? CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the branch name filter (supports partial match with *).
    /// Example: "Branch*" or "*Central"
    /// </summary>
    [FromQuery(Name = "branchName")]
    public string? BranchName { get; set; }

    /// <summary>
    /// Gets or sets the minimum date filter.
    /// Example: "2024-01-01"
    /// </summary>
    [FromQuery(Name = "_minDate")]
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum date filter.
    /// Example: "2024-12-31"
    /// </summary>
    [FromQuery(Name = "_maxDate")]
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// Gets or sets the minimum total amount filter.
    /// </summary>
    [FromQuery(Name = "_minTotalAmount")]
    public decimal? MinTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum total amount filter.
    /// </summary>
    [FromQuery(Name = "_maxTotalAmount")]
    public decimal? MaxTotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the cancelled status filter.
    /// If true, returns only cancelled sales.
    /// If false, returns only non-cancelled sales.
    /// If null, returns all sales.
    /// </summary>
    [FromQuery(Name = "isCancelled")]
    public bool? IsCancelled { get; set; }
}
