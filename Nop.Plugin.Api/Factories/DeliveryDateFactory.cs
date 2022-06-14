using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Api.Factories
{
    public class DeliveryDateFactory : IFactory<DeliveryDate>
    {
        public Task<DeliveryDate> InitializeAsync()
        {
            var deliveryDate = new DeliveryDate();
            return Task.FromResult(deliveryDate);
        }
    }
}
