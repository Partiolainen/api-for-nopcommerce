﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Topics;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.DeliveryDates;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.DTO.Manufacturers;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.DTO.ProductAttributes;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.DTO.SpecificationAttributes;
using Nop.Plugin.Api.DTO.Stores;
using Nop.Plugin.Api.DTO.Warehouses;
using Nop.Plugin.Api.DTOs.Topics;

namespace Nop.Plugin.Api.Helpers
{
    public interface IDTOHelper
    {
        Task<ProductDto> PrepareProductDTOAsync(Product product);
        Task<CategoryDto> PrepareCategoryDTOAsync(Category category);
        Task<OrderDto> PrepareOrderDTOAsync(Order order);
        Task<ShoppingCartItemDto> PrepareShoppingCartItemDTOAsync(ShoppingCartItem shoppingCartItem);
        Task<OrderItemDto> PrepareOrderItemDTOAsync(OrderItem orderItem);
        Task<StoreDto> PrepareStoreDTOAsync(Store store);
        Task<LanguageDto> PrepareLanguageDtoAsync(Language language);
        Task<CurrencyDto> PrepareCurrencyDtoAsync(Currency currency);
        ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute);
        ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute,
            SpecificationAttributeOption specificationAttributeOption);
        ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute,
            SpecificationAttributeOption specificationAttributeOption, SpecificationAttribute specificationAttribute);
        SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute,
            IList<SpecificationAttributeOption> specificationAttributeOptions);
        Task<ManufacturerDto> PrepareManufacturerDtoAsync(Manufacturer manufacturer);
        Task<WarehouseDto> PrepareWarehouseDtoAsync(Warehouse warehouse);
        DeliveryDateDto PrepareDeliveryDateDto(DeliveryDate deliveryDate);
        TopicDto PrepareTopicDTO(Topic topic);
        SpecificationAttributeOptionDto PrepareSpecificationAttributeOptionDto(SpecificationAttributeOption specificationAttributeOption);
    }
}