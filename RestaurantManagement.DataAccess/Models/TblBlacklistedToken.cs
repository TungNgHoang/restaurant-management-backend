using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.DataAccess.Models
{
    public class TblBlacklistedToken
    {
        public Guid Id { get; set; }  // Khóa chính
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }  // Thời gian hết hạn của token
    }
}
