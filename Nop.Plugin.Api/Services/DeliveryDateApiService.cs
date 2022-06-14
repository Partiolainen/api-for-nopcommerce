using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public class DeliveryDateApiService : IDeliveryDateApiService
    {
        private readonly IRepository<DeliveryDate> _deliveryDateRepository;

        public DeliveryDateApiService(IRepository<DeliveryDate> deliveryDateRepository)
        {
            _deliveryDateRepository = deliveryDateRepository;
        }
        public IList<DeliveryDate> GetDeliveryDates(IList<int> ids = null, string name = null)
        {
            var query = GetDeliveryDatesQuery(ids, name);

            return new ApiList<DeliveryDate>(query, 0, Constants.Configurations.MaxLimit);
        }

        public DeliveryDate GetDeliveryDateById(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var deliveryDate = _deliveryDateRepository.Table.FirstOrDefault(cat => cat.Id == id);

            return deliveryDate;
        }

        private IQueryable<DeliveryDate> GetDeliveryDatesQuery(IList<int> ids, string name)
        {
            var query = _deliveryDateRepository.Table;

            if (ids is { Count: > 0 })
            {
                query = query.Where(c => ids.Contains(c.Id));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query=query.Where(c => c.Name == name);
            }

            query = query.OrderBy(deliveryDate => deliveryDate.Id);

            return query;
        }
    }
}
