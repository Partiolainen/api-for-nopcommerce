using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Warehouses;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.CategoriesParameters;
using Nop.Plugin.Api.Models.WarehousesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using WarehousesRootObject = Nop.Plugin.Api.DTO.Warehouses.WarehousesRootObject;

namespace Nop.Plugin.Api.Controllers
{
    public class WarehousesController : BaseApiController
    {
        private readonly IWarehouseApiService _warehouseApiService;
        private readonly IShippingService _shippingService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IFactory<Warehouse> _factory;

        public WarehousesController(
            IWarehouseApiService warehouseApiService,
            IShippingService shippingService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IDTOHelper dtoHelper,
            IFactory<Warehouse> factory) : base(jsonFieldsSerializer,
            aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
            localizationService, pictureService)
        {
            _warehouseApiService = warehouseApiService;
            _shippingService = shippingService;
            _dtoHelper = dtoHelper;
            _factory = factory;
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

        /// <summary>
        ///     Retrieve warehouse by specified id
        /// </summary>
        /// <param name="id">Id of the warehouse</param>
        /// <param name="fields">Fields from the warehouse you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/warehouses/{id}", Name = "GetWarehouseById")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(WarehousesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetWarehouseById([FromRoute] int id, [FromQuery] string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var warehouse = _warehouseApiService.GetWarehouseById(id);

            if (warehouse == null)
            {
                return Error(HttpStatusCode.NotFound, "warehouse", "warehouse not found");
            }

            var warehouseDto = await _dtoHelper.PrepareWarehouseDtoAsync(warehouse);

            var warehousesRootObject = new WarehousesRootObject();

            warehousesRootObject.Warehouses.Add(warehouseDto);

            var json = JsonFieldsSerializer.Serialize(warehousesRootObject, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/warehouses", Name = "CreateWarehouse")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(WarehousesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateWarehouse(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<WarehouseDto>))]
            Delta<WarehouseDto> warehouseDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // Inserting the new warehouse
            var warehouse = await _factory.InitializeAsync();
            warehouseDelta.Merge(warehouse);

            await _shippingService.InsertWarehouseAsync(warehouse);
            
            await CustomerActivityService.InsertActivityAsync("AddNewWarehouse",
                                                   await LocalizationService.GetResourceAsync("ActivityLog.AddNewWarehouse"), warehouse);

            // Preparing the result dto of the new category
            var newWarehouseDto = await _dtoHelper.PrepareWarehouseDtoAsync(warehouse);

            var warehousesRootObject = new WarehousesRootObject();

            warehousesRootObject.Warehouses.Add(newWarehouseDto);

            var json = JsonFieldsSerializer.Serialize(warehousesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }
    }
}