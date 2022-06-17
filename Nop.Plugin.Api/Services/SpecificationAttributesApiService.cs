using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public class SpecificationAttributesApiService : ISpecificationAttributeApiService
    {
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributesRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributesRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;

        public SpecificationAttributesApiService(
            IRepository<ProductSpecificationAttribute> productSpecificationAttributesRepository,
            IRepository<SpecificationAttribute> specificationAttributesRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository)
        {
            _productSpecificationAttributesRepository = productSpecificationAttributesRepository;
            _specificationAttributesRepository = specificationAttributesRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
        }

        public IList<ProductSpecificationAttribute> GetProductSpecificationAttributes(
            int? productId = null, int? specificationAttributeOptionId = null, bool? allowFiltering = null,
            bool? showOnProductPage = null,
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId)
        {
            var query = _productSpecificationAttributesRepository.Table;

            if (productId > 0)
            {
                query = query.Where(psa => psa.ProductId == productId);
            }

            if (specificationAttributeOptionId > 0)
            {
                query = query.Where(psa => psa.SpecificationAttributeOptionId == specificationAttributeOptionId);
            }

            if (allowFiltering.HasValue)
            {
                query = query.Where(psa => psa.AllowFiltering == allowFiltering.Value);
            }

            if (showOnProductPage.HasValue)
            {
                query = query.Where(psa => psa.ShowOnProductPage == showOnProductPage.Value);
            }

            if (sinceId > 0)
            {
                query = query.Where(productAttribute => productAttribute.Id > sinceId);
            }

            query = query.OrderBy(x => x.Id);

            return new ApiList<ProductSpecificationAttribute>(query, page - 1, limit);
        }

        public IList<SpecificationAttribute> GetSpecificationAttributes(
            int limit = Constants.Configurations.DefaultLimit, int page = Constants.Configurations.DefaultPageValue,
            int sinceId = Constants.Configurations.DefaultSinceId)
        {
            var query = _specificationAttributesRepository.Table;

            if (sinceId > 0)
            {
                query = query.Where(x => x.Id > sinceId);
            }

            query = query.OrderBy(x => x.Id);

            return new ApiList<SpecificationAttribute>(query, page - 1, limit);
        }

        public IList<SpecificationAttributeOption> GetSpecificationAttributeOptions(int specificationAttributeId)
        {
            var query = _specificationAttributeOptionRepository.Table;
            query = query.OrderBy(x => x.Name);
            return new ApiList<SpecificationAttributeOption>(query, 0, Constants.Configurations.MaxLimit);
        }
    }
}