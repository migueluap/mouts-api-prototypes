using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

/// <summary>
/// Profile for mapping between API request/response and Application command/result for CancelSale.
/// </summary>
public class CancelSaleProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for CancelSale feature.
    /// </summary>
    public CancelSaleProfile()
    {
        // Mapping from Guid to CancelSaleCommand is no longer used
        // Command is now constructed manually in the controller to include RowVersion
        // CreateMap<Guid, CancelSaleCommand>()
        //     .ConstructUsing(id => new CancelSaleCommand(id, Array.Empty<byte>()));

        CreateMap<CancelSaleResult, CancelSaleResponse>();
    }
}
