using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantManagement.Service.Dtos.InvoiceDto
{
    public class InvoicePrintItemDto
    {
        public int Index { get; set; }           // STT
        public string Name { get; set; }         // Tên món
        public int Quantity { get; set; }        // Số lượng
        public decimal UnitPrice { get; set; }   // Đơn giá
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class InvoicePrintDto
    {
        public Guid OrderId { get; set; }
        public string InvoiceCode { get; set; } = string.Empty; // mã hóa đơn ngắn gọn
        public string StoreName { get; set; } = "PIZZADAY";
        public string StoreAddress { get; set; } = string.Empty;
        public string StorePhone { get; set; } = string.Empty;

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string TableNumber { get; set; }
        public DateTime InvoiceDate { get; set; }

        public List<InvoicePrintItemDto> Items { get; set; } = new();

        public decimal SubTotal { get; set; }           // before VAT and discounts
        public decimal VatRate { get; set; }            // e.g. 0.08m
        public decimal VatAmount { get; set; }
        public string VoucherCode { get; set; }         // nếu có
        public decimal VoucherDiscount { get; set; }    // positive number to subtract
        public decimal RankDiscount { get; set; }       // nếu có
        public decimal TotalAmount { get; set; }        // final amount customer pays (should match payment.Amount)
        public string PayMethod { get; set; }
    }
}
