using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;

namespace Nop.Plugin.Api.Factories
{
    public class CurrencyFactory : IFactory<Currency>
    {
        public Task<Currency> InitializeAsync()
        {
            var defaultCurrency = new Currency
            {
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            return Task.FromResult(defaultCurrency);
        }
    }
}
