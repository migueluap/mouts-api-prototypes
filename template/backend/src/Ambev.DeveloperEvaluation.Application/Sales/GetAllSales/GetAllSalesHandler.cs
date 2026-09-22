using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

/// <summary>
/// Handler for processing GetAllSalesQuery requests with filtering, sorting, and pagination.
/// </summary>
public class GetAllSalesHandler : IRequestHandler<GetAllSalesQuery, GetAllSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of GetAllSalesHandler.
    /// </summary>
    /// <param name="saleRepository">The sale repository</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public GetAllSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the GetAllSalesQuery request with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="query">The query containing filter, sort, and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated, filtered, and sorted list of sales</returns>
    public async Task<GetAllSalesResult> Handle(GetAllSalesQuery query, CancellationToken cancellationToken)
    {
        // Get all sales from repository (we'll filter and sort in memory)
        // For production, consider implementing filtering/sorting at database level
        var (allSales, _) = await _saleRepository.GetAllAsync(1, int.MaxValue, cancellationToken);

        // Apply filters
        var filteredSales = ApplyFilters(allSales.AsQueryable(), query);

        // Get total count after filtering
        var totalCount = filteredSales.Count();

        // Apply sorting
        var sortedSales = ApplySorting(filteredSales, query.Order);

        // Apply pagination
        var paginatedSales = sortedSales
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .ToList();

        // Map to summary results
        var saleSummaries = paginatedSales.Select(sale => new SaleSummaryResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            CustomerId = sale.CustomerId,
            CustomerName = sale.CustomerName,
            BranchId = sale.BranchId,
            BranchName = sale.BranchName,
            Date = sale.Date,
            TotalAmount = sale.TotalAmount,
            Cancelled = sale.Cancelled,
            ItemCount = sale.Items.Count
        }).ToList();

        return new GetAllSalesResult
        {
            Sales = saleSummaries,
            TotalCount = totalCount,
            CurrentPage = query.Page,
            PageSize = query.Size,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.Size)
        };
    }

    /// <summary>
    /// Applies filter criteria to the query.
    /// </summary>
    /// <param name="query">The queryable sales collection</param>
    /// <param name="filters">The filter parameters</param>
    /// <returns>Filtered queryable</returns>
    private IQueryable<Sale> ApplyFilters(IQueryable<Sale> query, GetAllSalesQuery filters)
    {
        // Filter by SaleNumber (supports partial match with *)
        if (!string.IsNullOrWhiteSpace(filters.SaleNumber))
        {
            query = ApplyStringFilter(query, s => s.SaleNumber, filters.SaleNumber);
        }

        // Filter by CustomerName (supports partial match with *)
        if (!string.IsNullOrWhiteSpace(filters.CustomerName))
        {
            query = ApplyStringFilter(query, s => s.CustomerName, filters.CustomerName);
        }

        // Filter by BranchName (supports partial match with *)
        if (!string.IsNullOrWhiteSpace(filters.BranchName))
        {
            query = ApplyStringFilter(query, s => s.BranchName, filters.BranchName);
        }

        // Filter by date range
        if (filters.MinDate.HasValue)
        {
            query = query.Where(s => s.Date >= filters.MinDate.Value);
        }

        if (filters.MaxDate.HasValue)
        {
            query = query.Where(s => s.Date <= filters.MaxDate.Value);
        }

        // Filter by total amount range
        if (filters.MinTotalAmount.HasValue)
        {
            query = query.Where(s => s.TotalAmount >= filters.MinTotalAmount.Value);
        }

        if (filters.MaxTotalAmount.HasValue)
        {
            query = query.Where(s => s.TotalAmount <= filters.MaxTotalAmount.Value);
        }

        // Filter by cancelled status
        if (filters.IsCancelled.HasValue)
        {
            query = query.Where(s => s.Cancelled == filters.IsCancelled.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies string filtering with wildcard support (* for partial match).
    /// </summary>
    /// <param name="query">The queryable sales collection</param>
    /// <param name="selector">The property selector</param>
    /// <param name="filterValue">The filter value (may contain *)</param>
    /// <returns>Filtered queryable</returns>
    private IQueryable<Sale> ApplyStringFilter(
        IQueryable<Sale> query,
        Expression<Func<Sale, string>> selector,
        string filterValue)
    {
        var compiledSelector = selector.Compile();

        if (filterValue.StartsWith("*") && filterValue.EndsWith("*"))
        {
            // Contains
            var value = filterValue.Trim('*');
            return query.Where(s => compiledSelector(s).Contains(value, StringComparison.OrdinalIgnoreCase));
        }
        else if (filterValue.StartsWith("*"))
        {
            // EndsWith
            var value = filterValue.TrimStart('*');
            return query.Where(s => compiledSelector(s).EndsWith(value, StringComparison.OrdinalIgnoreCase));
        }
        else if (filterValue.EndsWith("*"))
        {
            // StartsWith
            var value = filterValue.TrimEnd('*');
            return query.Where(s => compiledSelector(s).StartsWith(value, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // Exact match
            return query.Where(s => compiledSelector(s).Equals(filterValue, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Applies sorting to the query based on the order specification.
    /// </summary>
    /// <param name="query">The queryable sales collection</param>
    /// <param name="orderSpec">The order specification (e.g., "date desc, saleNumber asc")</param>
    /// <returns>Sorted queryable</returns>
    private IQueryable<Sale> ApplySorting(IQueryable<Sale> query, string? orderSpec)
    {
        if (string.IsNullOrWhiteSpace(orderSpec))
        {
            // Default sorting: most recent first
            return query.OrderByDescending(s => s.Date);
        }

        var orderParts = orderSpec.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Sale>? orderedQuery = null;

        foreach (var part in orderParts)
        {
            var trimmed = part.Trim();
            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = tokens[0].ToLowerInvariant();
            var direction = tokens.Length > 1 ? tokens[1].ToLowerInvariant() : "asc";

            var isDescending = direction == "desc";

            // Apply sorting based on field name
            orderedQuery = field switch
            {
                "id" => ApplyOrder(query, orderedQuery, s => s.Id, isDescending),
                "salenumber" => ApplyOrder(query, orderedQuery, s => s.SaleNumber, isDescending),
                "customername" => ApplyOrder(query, orderedQuery, s => s.CustomerName, isDescending),
                "branchname" => ApplyOrder(query, orderedQuery, s => s.BranchName, isDescending),
                "date" => ApplyOrder(query, orderedQuery, s => s.Date, isDescending),
                "totalamount" => ApplyOrder(query, orderedQuery, s => s.TotalAmount, isDescending),
                "cancelled" => ApplyOrder(query, orderedQuery, s => s.Cancelled, isDescending),
                "itemcount" => ApplyOrder(query, orderedQuery, s => s.Items.Count, isDescending),
                _ => orderedQuery
            };
        }

        return orderedQuery ?? query.OrderByDescending(s => s.Date);
    }

    /// <summary>
    /// Applies ordering (either initial or subsequent) to the query.
    /// </summary>
    private IOrderedQueryable<Sale> ApplyOrder<TKey>(
        IQueryable<Sale> query,
        IOrderedQueryable<Sale>? orderedQuery,
        Expression<Func<Sale, TKey>> keySelector,
        bool isDescending)
    {
        if (orderedQuery == null)
        {
            // First ordering
            return isDescending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }
        else
        {
            // Subsequent ordering (ThenBy)
            return isDescending
                ? orderedQuery.ThenByDescending(keySelector)
                : orderedQuery.ThenBy(keySelector);
        }
    }
}
