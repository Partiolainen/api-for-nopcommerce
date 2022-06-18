using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.SpecificationAttributes
{
    internal class SpecificationAttributeOptionsRootObjectDto : ISerializableObject
    {
        public SpecificationAttributeOptionsRootObjectDto()
        {
            SpecificationAttributeOptions = new List<SpecificationAttributeOptionDto>();
        }

        [JsonProperty("specification_attribute_options")]
        public IList<SpecificationAttributeOptionDto> SpecificationAttributeOptions { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "specification_attribute_options";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(SpecificationAttributeOptionDto);
        }
    }
}
