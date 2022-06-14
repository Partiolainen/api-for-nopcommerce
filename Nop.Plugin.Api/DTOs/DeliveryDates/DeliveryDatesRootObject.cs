using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.DeliveryDates
{
    public class DeliveryDatesRootObject : ISerializableObject
    {
        public DeliveryDatesRootObject()
        {
            DeliveryDates = new List<DeliveryDateDto>();
        }

        [JsonProperty("delivery_dates")]
        public IList<DeliveryDateDto> DeliveryDates { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "delivery_dates";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(DeliveryDateDto);
        }
    }
}
