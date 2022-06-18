using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.SpecificationAttributes;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class SpecificationAttributeDtoMappings
    {
        public static ProductSpecificationAttributeDto ToDto(
            this ProductSpecificationAttribute productSpecificationAttribute,
            SpecificationAttributeOption specificationAttributeOption)
        {
            var productSpecificationAttributeDto = productSpecificationAttribute
                .MapTo<ProductSpecificationAttribute, ProductSpecificationAttributeDto>();
            productSpecificationAttributeDto.SpecificationAttributeOption = specificationAttributeOption
                .MapTo<SpecificationAttributeOption, SpecificationAttributeOptionDto>();
            return productSpecificationAttributeDto;
        }

        public static SpecificationAttributeDto ToDto(this SpecificationAttribute specificationAttribute,
            IList<SpecificationAttributeOption> specificationAttributeOptions)
        {
            var attributeDto = specificationAttribute.MapTo<SpecificationAttribute, SpecificationAttributeDto>();
            attributeDto.SpecificationAttributeOptions = specificationAttributeOptions == null
                ? new List<SpecificationAttributeOptionDto>()
                : specificationAttributeOptions.ToList()
                    .MapTo<List<SpecificationAttributeOption>, List<SpecificationAttributeOptionDto>>();
            return attributeDto;
        }

        public static SpecificationAttributeOptionDto ToDto(
            this SpecificationAttributeOption specificationAttributeOption)
        {
            return specificationAttributeOption.MapTo<SpecificationAttributeOption, SpecificationAttributeOptionDto>();
        }

        public static SpecificationAttributeOption ToEntity(
            this SpecificationAttributeOptionDto specificationAttributeOptionDto)
        {
            return specificationAttributeOptionDto
                .MapTo<SpecificationAttributeOptionDto, SpecificationAttributeOption>();
        }
    }
}