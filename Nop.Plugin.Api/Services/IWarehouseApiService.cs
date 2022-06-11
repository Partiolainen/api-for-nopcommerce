using System.Collections.Generic;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services;

public interface IWarehouseApiService
{
    IList<Warehouse> GetWarehouses(IList<int> ids = null,
        int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
        int? productId = null);

    Warehouse GetWarehouseById(int id);
}