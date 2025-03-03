using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos
{
    public class ReservationDto
    {
        public Guid ResID { get; set; }  // Có thể để rỗng khi tạo mới

        [Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
        public Guid CusID { get; set; }

        [Required(ErrorMessage = "Mã bàn là bắt buộc.")]
        public Guid TbiID { get; set; }

        [Required(ErrorMessage = "Ngày đặt bàn là bắt buộc.")]
        public DateTime ResDate { get; set; }

        [Required(ErrorMessage = "Giờ đặt bàn là bắt buộc.")]
        public TimeSpan ResTime { get; set; }

        [Required(ErrorMessage = "Số lượng người đặt bàn là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Số lượng người phải từ 1 đến 100.")]
        public int ResNumber { get; set; }

        public string? ResStatus { get; set; }
    }
}
