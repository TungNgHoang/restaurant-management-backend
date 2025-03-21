using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Interfaces
{
    public interface IPaymentService
    {
        Task CheckoutAndPayAsync(Guid resId, Guid ordId, string payMethod);
    }
}
