using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.OrdersDto
{
    public class CreateOrderRequestDto
    {
        public Guid CustomerId { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
    }
}
