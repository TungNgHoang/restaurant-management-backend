using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.MenusDto
{
    public class MenuDto
    {
        public Guid MnuId { get; set; }
        public string MnuName { get; set; }
        public decimal MnuPrice { get; set; }
        public string MnuStatus { get; set; }
        public string MnuImage { get; set; }
        public string MnuDescription { get; set; }
    }
}
