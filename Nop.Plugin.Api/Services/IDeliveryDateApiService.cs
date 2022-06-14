using System.Collections.Generic;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Api.Services;

public interface IDeliveryDateApiService
{
    IList<DeliveryDate> GetDeliveryDates(IList<int> ids = null, string name = null);
    DeliveryDate GetDeliveryDateById(int id);
}