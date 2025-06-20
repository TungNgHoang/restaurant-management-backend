using RestaurantManagement.Service.Dtos.OrdersDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.ApiModels
{
    public class OrderRequest
    {

        public class ProcessOrderRequest
        {
            public Guid TbiId { get; set; }
            public List<OrderItemDto> NewOrderItems { get; set; }
        }

        public class ProcessPreOrderRequest
        {
            public Guid ResId { get; set; }
            public List<OrderItemDto> NewOrderItems { get; set; }
        }
    }
}
