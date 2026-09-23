using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core.
/// Provides data access operations for Sale aggregate root and its SaleItems.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of SaleRepository.
    /// </summary>
    /// <param name="context">The database context</param>
    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new sale in the database.
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale with generated ID and RowVersion</returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // Generate initial RowVersion for the sale (PostgreSQL doesn't auto-generate bytea)
        sale.RowVersion = Guid.NewGuid().ToByteArray();

        // Generate initial RowVersion for each sale item
        foreach (var item in sale.Items)
        {
            item.RowVersion = Guid.NewGuid().ToByteArray();
        }

        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return sale;
    }

    /// <summary>
    /// Retrieves a sale by its unique identifier, including all sale items.
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a sale by its sale number.
    /// </summary>
    /// <param name="saleNumber">The sale number to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    /// <summary>
    /// Retrieves all sales with optional pagination.
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="size">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the list of sales and the total count</returns>
    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetAllAsync(
        int page = 1,
        int size = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var sales = await query
            .OrderByDescending(s => s.Date)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    /// <summary>
    /// Retrieves sales for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of sales for the specified customer</returns>
    public async Task<IEnumerable<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves sales for a specific branch.
    /// </summary>
    /// <param name="branchId">The branch ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of sales for the specified branch</returns>
    public async Task<IEnumerable<Sale>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves sales within a date range.
    /// </summary>
    /// <param name="startDate">Start date of the range</param>
    /// <param name="endDate">End date of the range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of sales within the specified date range</returns>
    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing sale in the repository.
    /// Note: EF Core's change tracking will detect modifications to the sale and its items.
    /// Implements Optimistic Concurrency Control by validating RowVersion before update.
    /// </summary>
    /// <param name="sale">The sale with updated information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sale with new RowVersion</returns>
    /// <exception cref="DbUpdateConcurrencyException">Thrown when RowVersion mismatch occurs</exception>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // Attach the entity to the context if not already tracked
        var entry = _context.Entry(sale);
        if (entry.State == EntityState.Detached)
        {
            _context.Sales.Attach(sale);
            entry = _context.Entry(sale);
        }

        // Mark the entity as modified
        entry.State = EntityState.Modified;

        // Store the ORIGINAL RowVersion (the one sent by client for concurrency check)
        // This is what EF Core will use in the WHERE clause
        var originalRowVersion = sale.RowVersion;

        // Generate NEW RowVersion for the sale (PostgreSQL doesn't auto-generate bytea)
        // This is what will be saved to the database if the update succeeds
        var newRowVersion = Guid.NewGuid().ToByteArray();
        sale.RowVersion = newRowVersion;

        // Tell EF Core to use the ORIGINAL value in the WHERE clause for concurrency check
        // SQL will be: UPDATE Sales SET RowVersion=@new WHERE Id=@id AND RowVersion=@original
        entry.OriginalValues[nameof(Sale.RowVersion)] = originalRowVersion;

        // Process items: handle both existing (modified) and new items
        foreach (var item in sale.Items)
        {
            var itemEntry = _context.Entry(item);

            if (itemEntry.State == EntityState.Added)
            {
                // New item - just set initial RowVersion
                item.RowVersion = Guid.NewGuid().ToByteArray();
            }
            else
            {
                // Existing item - apply same concurrency pattern
                var originalItemRowVersion = item.RowVersion;
                var newItemRowVersion = Guid.NewGuid().ToByteArray();
                item.RowVersion = newItemRowVersion;
                itemEntry.OriginalValues[nameof(SaleItem.RowVersion)] = originalItemRowVersion;
            }
        }

        // SaveChanges will throw DbUpdateConcurrencyException if RowVersion doesn't match
        await _context.SaveChangesAsync(cancellationToken);

        return sale;
    }

    /// <summary>
    /// Deletes a sale from the repository (physical delete).
    /// Note: For business purposes, consider using Cancel() on the Sale entity instead.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the sale was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale == null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
