using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.AuthDto
{
    public class UserAccountDto
    {
        public Guid UacId { get; set; }
        public string UacUsername { get; set; }
        public string UacPassword { get; set; }
        public string UacRole { get; set; }
    }
}
