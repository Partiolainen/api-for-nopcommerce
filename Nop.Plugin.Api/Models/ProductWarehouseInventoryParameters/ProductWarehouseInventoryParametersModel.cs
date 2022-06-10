using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Models.ProductWarehouseInventoryParameters
{
    public class ProductWarehouseInventoryParametersModel
    {
        public ProductWarehouseInventoryParametersModel()
        {
            Fields = string.Empty;
        }

        /// <summary>
        ///     Show all the product-category mappings for this product
        /// </summary>
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        /// <summary>
        ///     comma-separated list of fields to include in the response
        /// </summary>
        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}
