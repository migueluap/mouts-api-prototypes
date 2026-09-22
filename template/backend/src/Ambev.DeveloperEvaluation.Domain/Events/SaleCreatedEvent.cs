using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a new sale is created.
/// This event can be used for logging, notifications, or triggering other business processes.
/// </summary>
public class SaleCreatedEvent : DomainEventBase
{
    /// <summary>
    /// Gets the ID of the created sale.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale number.
    /// </summary>
    public string SaleNumber { get; }

    /// <summary>
    /// Gets the customer ID.
    /// </summary>
    public Guid CustomerId { get; }

    /// <summary>
    /// Gets the customer name.
    /// </summary>
    public string CustomerName { get; }

    /// <summary>
    /// Gets the branch ID.
    /// </summary>
    public Guid BranchId { get; }

    /// <summary>
    /// Gets the branch name.
    /// </summary>
    public string BranchName { get; }

    /// <summary>
    /// Gets the total amount of the sale.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    /// Gets the number of items in the sale.
    /// </summary>
    public int ItemCount { get; }

    /// <summary>
    /// Initializes a new instance of SaleCreatedEvent.
    /// </summary>
    /// <param name="sale">The sale that was created</param>
    public SaleCreatedEvent(Sale sale)
    {
        SaleId = sale.Id;
        SaleNumber = sale.SaleNumber;
        CustomerId = sale.CustomerId;
        CustomerName = sale.CustomerName;
        BranchId = sale.BranchId;
        BranchName = sale.BranchName;
        TotalAmount = sale.TotalAmount;
        ItemCount = sale.Items.Count;
    }
}
