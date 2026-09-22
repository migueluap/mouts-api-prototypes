using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.GetAllSales;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetAllSales;

/// <summary>
/// Profile for mapping between API request/response and Application query/result for GetAllSales.
/// </summary>
public class GetAllSalesProfile : Profile
{
    /// <summary>
    /// Initializes the mappings for GetAllSales feature with filtering and ordering support.
    /// </summary>
    public GetAllSalesProfile()
    {
        CreateMap<GetAllSalesRequest, GetAllSalesQuery>();

        CreateMap<GetAllSalesResult, GetAllSalesResponse>();

        CreateMap<SaleSummaryResult, SaleSummaryResponse>();
    }
}
