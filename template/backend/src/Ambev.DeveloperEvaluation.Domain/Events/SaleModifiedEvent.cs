using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a sale is modified.
/// This includes changes to sale items, quantities, or other sale properties.
/// </summary>
public class SaleModifiedEvent : DomainEventBase
{
    /// <summary>
    /// Gets the ID of the modified sale.
    /// </summary>
    public Guid SaleId { get; }

    /// <summary>
    /// Gets the sale number.
    /// </summary>
    public string SaleNumber { get; }

    /// <summary>
    /// Gets a description of what was modified.
    /// </summary>
    public string ModificationDescription { get; }

    /// <summary>
    /// Gets the new total amount after modification.
    /// </summary>
    public decimal NewTotalAmount { get; }

    /// <summary>
    /// Gets the new item count after modification.
    /// </summary>
    public int NewItemCount { get; }

    /// <summary>
    /// Initializes a new instance of SaleModifiedEvent.
    /// </summary>
    /// <param name="sale">The sale that was modified</param>
    /// <param name="modificationDescription">Description of the modification</param>
    public SaleModifiedEvent(Sale sale, string modificationDescription)
    {
        SaleId = sale.Id;
        SaleNumber = sale.SaleNumber;
        ModificationDescription = modificationDescription;
        NewTotalAmount = sale.TotalAmount;
        NewItemCount = sale.Items.Count;
    }
}
