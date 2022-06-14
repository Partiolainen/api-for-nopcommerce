using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.DeliveryDatesParameters
{
    public class DeliveryDatesParametersModel
    {
        [JsonProperty("ids")]
        public List<int> Ids { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}