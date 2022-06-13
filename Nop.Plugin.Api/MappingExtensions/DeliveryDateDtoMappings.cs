using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class DeliveryDateDtoMappings 
    {
        public static DeliveryDateDto ToDto(this DeliveryDate deliveryDate)
        {
            return deliveryDate.MapTo<DeliveryDate, DeliveryDateDto>();
        }

        public static DeliveryDate ToEntity(this DeliveryDateDto deliveryDateDto)
        {
            return deliveryDateDto.MapTo<DeliveryDateDto, DeliveryDate>();
        }
    }
}
