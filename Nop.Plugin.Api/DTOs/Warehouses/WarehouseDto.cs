using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO.Warehouses
{
    public class WarehouseDto : BaseDto
    {
        /// <summary>
        /// Gets or sets the warehouse name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the address identifier of the warehouse
        /// </summary>
        public AddressDto Address { get; set; }
    }
}
