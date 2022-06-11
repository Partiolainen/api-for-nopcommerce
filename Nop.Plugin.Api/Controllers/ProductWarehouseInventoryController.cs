﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.ProductWarehouseIventories;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Models.ProductWarehouseInventoryParameters;
using Nop.Plugin.Api.Services;
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
        private readonly IProductWarehouseInventoriesApiService _productWarehouseInventoriesService;

        public ProductWarehouseInventoryController(
            IProductService productService,
            IProductWarehouseInventoriesApiService productWarehouseInventoriesService,
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
            _productWarehouseInventoriesService = productWarehouseInventoriesService;
        }

        /// <summary>
        ///     Receive a list of all Product-Warehouse inventory
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/product_warehouse_inventories", Name = "GetProductCategoryInventories")]
        [ProducesResponseType(typeof(ProductWarehouseInventoryRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetInventories([FromQuery] ProductWarehouseInventoryParametersModel parameters)
        {
            if (parameters.Limit < Constants.Configurations.MinLimit || parameters.Limit > Constants.Configurations.MaxLimit)
            {
                return Error(HttpStatusCode.BadRequest, "limit", "invalid limit parameter");
            }

            if (parameters.Page < Constants.Configurations.DefaultPageValue)
            {
                return Error(HttpStatusCode.BadRequest, "page", "invalid page parameter");
            }

            IList<ProductWarehouseInventoryDto> inventoryDtos =
                _productWarehouseInventoriesService.GetMappings(parameters.ProductId,
                    parameters.WarehouseId,
                    parameters.Limit,
                    parameters.Page,
                    parameters.SinceId).Select(x => x.ToDto()).ToList();

            var productWarehouseInventoryRootObject = new ProductWarehouseInventoryRootObject()
            {
                ProductWarehouseInventoryDtos = inventoryDtos
            };

            var json = JsonFieldsSerializer.Serialize(productWarehouseInventoryRootObject, parameters.Fields);

            return new RawJsonActionResult(json);
        }

        /// <summary>
        ///     Retrieve Product-Warehouse inventories by specified id
        /// </summary>
        /// ///
        /// <param name="id">Id of the Product-Warehouse inventory</param>
        /// <param name="fields">Fields from the Product-Warehouse inventory you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/product_warehouse_inventories/{id}", Name = "GetProductWarehouseInventoriesById")]
        [ProducesResponseType(typeof(ProductWarehouseInventoryRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetInventoryById([FromRoute] int id, [FromQuery] string fields = "")
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var mapping = await _productWarehouseInventoriesService.GetByIdAsync(id);

            if (mapping == null)
            {
                return Error(HttpStatusCode.NotFound, "product_warehouse_inventories", "not found");
            }

            var productWarehouseInventoryRootObject = new ProductWarehouseInventoryRootObject();
            productWarehouseInventoryRootObject.ProductWarehouseInventoryDtos.Add(mapping.ToDto());

            var json = JsonFieldsSerializer.Serialize(productWarehouseInventoryRootObject, fields);

            return new RawJsonActionResult(json);
        }
    }
}