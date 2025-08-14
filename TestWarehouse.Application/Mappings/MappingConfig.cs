using AutoMapper;
using TestWarehouse.Application.DTO;
using TestWarehouse.Domain.Entities;

namespace TestWarehouse.Application.Mappings;

public class MappingConfig : Profile
{
    public MappingConfig()
    {
        CreateMap<Resource, ResourceDto>();
        CreateMap<ResourceEditDto, Resource>();

        CreateMap<Unit, UnitDto>();
        CreateMap<UnitEditDto, Unit>();

        CreateMap<Balance, BalanceDto>()
            .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => src.Resource.Name))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));

        CreateMap<DocumentItem, DocumentItemDto>()
            .ForMember(dest => dest.ResourceName, opt => opt.MapFrom(src => src.Resource.Name))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));
        CreateMap<DocumentItemDto, DocumentItem>();

        CreateMap<ReceiptDocument, ReceiptDto>();
        CreateMap<ReceiptEditDto, ReceiptDocument>();

        CreateMap<ShipmentDocument, ShipmentDto>();
        CreateMap<ShipmentEditDto, ShipmentDocument>();
    }
}
