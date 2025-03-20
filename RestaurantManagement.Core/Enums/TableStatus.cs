using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Core.Enums
{
    public enum TableStatus
    {
        [Description("Empty")]
        Empty = 0,
        [Description("Occupied")]
        Occupied = 1
    }
}
