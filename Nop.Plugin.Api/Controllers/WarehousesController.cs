using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Warehouses;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.CategoriesParameters;
using Nop.Plugin.Api.Models.WarehousesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class WarehousesController : BaseApiController
    {
        private readonly IWarehouseApiService _warehouseApiService;
        private readonly IDTOHelper _dtoHelper;

        public WarehousesController(
            IWarehouseApiService warehouseApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IDTOHelper dtoHelper) : base(jsonFieldsSerializer,
            aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
            localizationService, pictureService)
        {
            _warehouseApiService = warehouseApiService;
            _dtoHelper = dtoHelper;
        }

        /// <summary>
        ///     Receive a list of all Warehouses
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/warehouses", Name = "GetWarehouses")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(WarehousesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetWarehouses([FromQuery] WarehousesParametersModel parameters)
        {
            if (parameters.Limit < Constants.Configurations.MinLimit || parameters.Limit > Constants.Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "Invalid limit parameter");
            }

            if (parameters.Page < Constants.Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "Invalid page parameter");
            }

            var allWarehouses = _warehouseApiService.GetWarehouses(parameters.Ids, 
                                                                  parameters.Limit, parameters.Page, parameters.ProductId);

            IList<WarehouseDto> warehousesAsDtos = await allWarehouses
                .SelectAwait(async warehouse => await _dtoHelper.PrepareWarehouseDtoAsync(warehouse)).ToListAsync();

            var warehousesRootObject = new WarehousesRootObject
            {
                Warehouses = warehousesAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(warehousesRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }
    }
}