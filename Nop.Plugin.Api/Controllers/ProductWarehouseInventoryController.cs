using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.ProductWarehouseIventories;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Models.ProductWarehouseInventoryParameters;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    [AuthorizePermission("ManageProducts")]
    public class ProductWarehouseInventoryController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductWarehouseInventoryController(
            IProductService productService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService,
                discountService, customerActivityService, localizationService, pictureService)
        {
            _productService = productService;
        }

        /// <summary>
        ///     Receive a list of all Product-Warehouse inventory
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/product_warehouse_inventory", Name = "GetProductCategoryInventories")]
        [ProducesResponseType(typeof(ProductWarehouseInventoryRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetInventories([FromQuery] ProductWarehouseInventoryParametersModel parameters)
        {
           
            IList<ProductWarehouseInventoryDto> inventoryDtos =
                    (await _productService.GetAllProductWarehouseInventoryRecordsAsync(parameters.ProductId))
                    .Select(x=>x.ToDto())
                    .ToList();
            
            var productWarehouseInventoryRootObject = new ProductWarehouseInventoryRootObject()
            {
                ProductWarehouseInventoryDtos = inventoryDtos
            };

            var json = JsonFieldsSerializer.Serialize(productWarehouseInventoryRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }
    }
}