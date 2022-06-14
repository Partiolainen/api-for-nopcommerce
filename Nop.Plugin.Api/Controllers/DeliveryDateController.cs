using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.DeliveryDates;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.DeliveryDatesParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class DeliveryDateController : BaseApiController
    {
        private readonly IDeliveryDateApiService _deliveryDateApiService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IFactory<DeliveryDate> _factory;

        public DeliveryDateController(
            IDeliveryDateApiService deliveryDateApiService,
            IDateRangeService dateRangeService,
            IDTOHelper dtoHelper,
            IFactory<DeliveryDate> factory,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService) : base(jsonFieldsSerializer, aclService, customerService,
            storeMappingService, storeService, discountService, customerActivityService, localizationService,
            pictureService)
        {
            _deliveryDateApiService = deliveryDateApiService;
            _dateRangeService = dateRangeService;
            _dtoHelper = dtoHelper;
            _factory = factory;
        }

        /// <summary>
        ///     Receive a list of all delivery dates
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/delivery-dates", Name = "GetDeliveryDates")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(DeliveryDatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetDeliveryDates([FromQuery] DeliveryDatesParametersModel parameters)
        {
            var allDeliveryDates = _deliveryDateApiService.GetDeliveryDates(parameters.Ids, parameters.Name);

            IList<DeliveryDateDto> deliveryDatesAsDtos = await allDeliveryDates
                .Select(deliveryDate => _dtoHelper.PrepareDeliveryDateDto(deliveryDate)).ToListAsync();

            var deliveryDatesRootObject = new DeliveryDatesRootObject()
            {
                DeliveryDates = deliveryDatesAsDtos
            };

            var json = JsonFieldsSerializer.Serialize(deliveryDatesRootObject, null);

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
        [Route("/api/delivery-dates/{id}", Name = "GetDeliveryDatesById")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(DeliveryDatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetDeliveryDatesById([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var deliveryDate = _deliveryDateApiService.GetDeliveryDateById(id);

            if (deliveryDate == null)
            {
                return Error(HttpStatusCode.NotFound, "deliveryDate", "deliveryDate not found");
            }

            var deliveryDateDto = _dtoHelper.PrepareDeliveryDateDto(deliveryDate);

            var deliveryDatesRootObject = new DeliveryDatesRootObject();

            deliveryDatesRootObject.DeliveryDates.Add(deliveryDateDto);

            var json = JsonFieldsSerializer.Serialize(deliveryDatesRootObject, null);

            return new RawJsonActionResult(json);
        }

        [HttpPost]
        [Route("/api/delivery-dates", Name = "CreateDeliveryDate")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(DeliveryDatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateDeliveryDate(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<DeliveryDateDto>))]
            Delta<DeliveryDateDto> deliveryDateDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // Inserting the new delivery date
            var deliveryDate = await _factory.InitializeAsync();

            deliveryDateDelta.Merge(deliveryDate);

            await _dateRangeService.InsertDeliveryDateAsync(deliveryDate);

            await CustomerActivityService.InsertActivityAsync("AddNewDeliveryDate",
                                                   await LocalizationService.GetResourceAsync("ActivityLog.AddNewDeliveryDate"), deliveryDate);

            // Preparing the result dto of the new category
            var newDeliveryDateDto = _dtoHelper.PrepareDeliveryDateDto(deliveryDate);

            var deliveryDatesRootObject = new DeliveryDatesRootObject();

            deliveryDatesRootObject.DeliveryDates.Add(newDeliveryDateDto);

            var json = JsonFieldsSerializer.Serialize(deliveryDatesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/delivery-dates/{id}", Name = "UpdateDeliveryDate")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(DeliveryDatesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateDeliveryDate(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<DeliveryDateDto>))]
            Delta<DeliveryDateDto> deliveryDateDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(deliveryDateDelta.Dto.Id);

            if (deliveryDate == null)
            {
                return Error(HttpStatusCode.NotFound, "warehouse", "warehouse not found");
            }

            deliveryDateDelta.Merge(deliveryDate);

            await _dateRangeService.UpdateDeliveryDateAsync(deliveryDate);

            await CustomerActivityService.InsertActivityAsync("UpdateDeliveryDate",
                                                   await LocalizationService.GetResourceAsync("ActivityLog.UpdateDeliveryDate"), deliveryDate);

            var deliveryDateDto = _dtoHelper.PrepareDeliveryDateDto(deliveryDate);

            var deliveryDatesRootObject = new DeliveryDatesRootObject();

            deliveryDatesRootObject.DeliveryDates.Add(deliveryDateDto);

            var json = JsonFieldsSerializer.Serialize(deliveryDatesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/delivery-dates/{id}", Name = "DeleteDeliveryDate")]
        [AuthorizePermission("ManageShippingSettings")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteDeliveryDate([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var deliveryDateToDelete = await _dateRangeService.GetDeliveryDateByIdAsync(id);

            if (deliveryDateToDelete == null)
            {
                return Error(HttpStatusCode.NotFound, "deliveryDate", "deliveryDate not found");
            }

            await _dateRangeService.DeleteDeliveryDateAsync(deliveryDateToDelete);

            //activity log
            await CustomerActivityService.InsertActivityAsync("DeleteDeliveryDate", 
                await LocalizationService.GetResourceAsync("ActivityLog.DeleteDeliveryDate"), deliveryDateToDelete);

            return new RawJsonActionResult("{}");
        }
    }
}