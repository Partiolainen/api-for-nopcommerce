using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Services;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class CurrenciesController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly ICustomerApiService _customerApiService;
        private readonly IFactory<Currency> _factory;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrencyService _currencyService;

        public CurrenciesController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICurrencyService currencyService,
            IDTOHelper dtoHelper,
            ICustomerApiService customerApiService,
            IFactory<Currency> factory,
            IAuthenticationService authenticationService)
            : base(jsonFieldsSerializer,
                aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _currencyService = currencyService;
            _dtoHelper = dtoHelper;
            _customerApiService = customerApiService;
            _factory = factory;
            _authenticationService = authenticationService;
        }

        /// <summary>
        ///     Retrieve all currencies
        /// </summary>
        /// <param name="fields">Fields from the language you want your json to contain</param>
        [HttpGet]
        [Route("/api/currencies", Name = "GetAllCurrencies")]
        [ProducesResponseType(typeof(CurrenciesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetAllCurrencies([FromQuery] int? storeId = null,
            [FromQuery] string fields = null, [FromQuery] bool showHidden = false)
        {
            // no permissions required

            var allCurrencies = await _currencyService.GetAllCurrenciesAsync(storeId: storeId ?? 0, showHidden: showHidden);

            IList<CurrencyDto> currenciesAsDto = await allCurrencies
                .SelectAwait(async language => await _dtoHelper.PrepareCurrencyDtoAsync(language)).ToListAsync();

            var currenciesRootObject = new CurrenciesRootObject
            {
                Currencies = currenciesAsDto
            };

            var json = JsonFieldsSerializer.Serialize(currenciesRootObject, fields ?? "");

            return new RawJsonActionResult(json);
        }

        [HttpGet]
        [Route("/api/currencies/primary", Name = "GetPrimaryCurrency")]
        [ProducesResponseType(typeof(CurrencyDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetPrimaryCurrency([FromServices] CurrencySettings currencySettings)
        {
            var primaryStoreCurrency =
                await _currencyService.GetCurrencyByIdAsync(currencySettings.PrimaryStoreCurrencyId);
            if (primaryStoreCurrency is null)
                return NoContent();
            var currencyDto = await _dtoHelper.PrepareCurrencyDtoAsync(primaryStoreCurrency);
            return Ok(currencyDto);
        }

        [HttpGet]
        [Route("/api/currencies/current", Name = "GetCurrentCurrency")]
        [ProducesResponseType(typeof(CurrencyDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetCurrentCurrency()
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return Error(HttpStatusCode.Unauthorized);
            // no permissions required
            var currency = await _customerApiService.GetCustomerCurrencyAsync(customer);
            if (currency is null)
                return NoContent();
            var currencyDto = await _dtoHelper.PrepareCurrencyDtoAsync(currency);
            return Ok(currencyDto);
        }

        [HttpPost]
        [Route("/api/currencies/current", Name = "SetCurrentCurrency")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> SetCurrentCurrency([FromQuery] int id)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return Error(HttpStatusCode.Unauthorized);
            // no permissions required
            var currency = await _currencyService.GetCurrencyByIdAsync(id);
            if (currency is null)
                return NotFound();
            await _customerApiService.SetCustomerCurrencyAsync(customer, currency);
            return NoContent();
        }

        [HttpPost]
        [Route("/api/currencies", Name = "CreateCurrency")]
        [AuthorizePermission("ManageCurrencies")]
        [ProducesResponseType(typeof(CurrenciesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> CreateCategory(
            [FromBody] [ModelBinder(typeof(JsonModelBinder<CurrencyDto>))]
            Delta<CurrencyDto> currencyDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // Inserting the new category
            var currency = await _factory.InitializeAsync();
            currencyDelta.Merge(currency);

            await _currencyService.InsertCurrencyAsync(currency);


            await CustomerActivityService.InsertActivityAsync("AddNewCurrency",
                await LocalizationService.GetResourceAsync("ActivityLog.AddNewCurrency"), currency);

            // Preparing the result dto of the new category
            var newCurrencyDto = await _dtoHelper.PrepareCurrencyDtoAsync(currency);

            var currenciesRootObject = new CurrenciesRootObject();

            currenciesRootObject.Currencies.Add(newCurrencyDto);

            var json = JsonFieldsSerializer.Serialize(currenciesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/currencies/{id}", Name = "UpdateCurrency")]
        [AuthorizePermission("ManageCurrencies")]
        [ProducesResponseType(typeof(CurrenciesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateCurrency(
            [FromBody] [ModelBinder(typeof(JsonModelBinder<CurrencyDto>))]
            Delta<CurrencyDto> currencyDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var currency = await _currencyService.GetCurrencyByIdAsync(currencyDelta.Dto.Id);

            if (currency == null)
            {
                return Error(HttpStatusCode.NotFound, "currency", "currency not found");
            }

            currencyDelta.Merge(currency);

            currency.UpdatedOnUtc = DateTime.UtcNow;

            await _currencyService.UpdateCurrencyAsync(currency);

            await CustomerActivityService.InsertActivityAsync("UpdateCurrency",
                await LocalizationService.GetResourceAsync("ActivityLog.UpdateCurrency"), currency);

            var currencyDto = await _dtoHelper.PrepareCurrencyDtoAsync(currency);

            var currenciesRootObject = new CurrenciesRootObject();

            currenciesRootObject.Currencies.Add(currencyDto);

            var json = JsonFieldsSerializer.Serialize(currenciesRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/currencies/{id}", Name = "DeleteCurrency")]
        [AuthorizePermission("ManageCurrencies")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteCurrency([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var currencyToDelete = await _currencyService.GetCurrencyByIdAsync(id);

            if (currencyToDelete == null)
            {
                return Error(HttpStatusCode.NotFound, "currency", "currency not found");
            }

            await _currencyService.DeleteCurrencyAsync(currencyToDelete);

            //activity log
            await CustomerActivityService.InsertActivityAsync("DeleteCurrency",
                await LocalizationService.GetResourceAsync("ActivityLog.DeleteCurrency"), currencyToDelete);

            return new RawJsonActionResult("{}");
        }
    }
}