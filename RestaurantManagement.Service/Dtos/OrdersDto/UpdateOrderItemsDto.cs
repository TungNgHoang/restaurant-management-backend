using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.OrdersDto
{
    public class UpdateOrderItemsDto
    {

        public Guid OrdID { get; set; }
        public List<OrderItemDto> OrderDetails { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid MnuID { get; set; }
        public int OdtQuantity { get; set; }
    }

}
