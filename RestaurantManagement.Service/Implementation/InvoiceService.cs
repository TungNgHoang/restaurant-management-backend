using iTextSharp.text;
using iTextSharp.text.pdf;


namespace RestaurantManagement.Service.Implementation
{
    public class InvoiceService : BaseService, IInvoiceService
    {
        private readonly IRepository<TblTableInfo> _tableRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblOrderInfo> _orderInfoRepository;
        private readonly IRepository<TblPayment> _paymentRepository;
        private readonly IRepository<TblOrderDetail> _orderDetailRepository;
        private readonly IRepository<TblMenu> _menuRepository;
        private readonly RestaurantDBContext _dbContext;

        public InvoiceService(
            AppSettings appSettings,
            IMapper mapper,
            IRepository<TblTableInfo> tableRepository,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblOrderInfo> orderInfoRepository,
            IRepository<TblPayment> paymentRepository,
            IRepository<TblOrderDetail> orderDetailRepository,
            IRepository<TblMenu> menuRepository,
            IHttpContextAccessor httpContextAccessor,
            RestaurantDBContext dbContext
        ) : base(appSettings, mapper, httpContextAccessor, dbContext)
        {
            _tableRepository = tableRepository;
            _reservationsRepository = reservationsRepository;
            _orderInfoRepository = orderInfoRepository;
            _paymentRepository = paymentRepository;
            _orderDetailRepository = orderDetailRepository;
            _menuRepository = menuRepository;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoiceAsync(InvoiceModels pagingModel)
        {
            // Logic hiện tại của bạn
            var reservations = await _reservationsRepository.AsNoTrackingAsync();
            var tables = await _tableRepository.AsNoTrackingAsync();
            var order = await _orderInfoRepository.AsNoTrackingAsync();
            var payment = await _paymentRepository.AsNoTrackingAsync();

            var data = from o in order
                       join r in reservations on o.ResId equals r.ResId
                       join t in tables on r.TbiId equals t.TbiId
                       join p in payment on o.OrdId equals p.OrdId
                       select new
                       {
                           t.TbiTableNumber,
                           r.TempCustomerName,
                           r.TempCustomerPhone,
                           r.ResDate,
                           r.ResEndTime,
                           r.ResNumber,
                           o.TotalPrice,
                           p.PayMethod
                       };

            var invoiceDto = data.Select(x => new InvoiceDto
            {
                TableNumber = x.TbiTableNumber,
                CustomerName = x.TempCustomerName,
                CustomerPhone = x.TempCustomerPhone,
                Date = x.ResDate.Date,
                TimeIn = x.ResDate.TimeOfDay,
                TimeOut = x.ResEndTime?.TimeOfDay ?? TimeSpan.Zero,
                People = x.ResNumber,
                TotalPrice = x.TotalPrice,
                PayMethod = x.PayMethod,
            }).ToList();

            var result = AdvancedFilter(invoiceDto.AsEnumerable(), pagingModel, nameof(InvoiceDto.TimeOut));
            return result;
        }

        public async Task<byte[]> GenerateInvoicePdf(Guid orderId)
        {
            // Load dữ liệu
            var orders = await _orderInfoRepository.AsNoTrackingAsync();
            var reservations = await _reservationsRepository.AsNoTrackingAsync();
            var tables = await _tableRepository.AsNoTrackingAsync();
            var payments = await _paymentRepository.AsNoTrackingAsync();
            var orderDetails = await _orderDetailRepository.AsNoTrackingAsync();
            var menus = await _menuRepository.AsNoTrackingAsync();

            var query = from o in orders
                        join r in reservations on o.ResId equals r.ResId
                        join t in tables on r.TbiId equals t.TbiId
                        join p in payments on o.OrdId equals p.OrdId
                        where o.OrdId == orderId
                        select new
                        {
                            Order = o,
                            Reservation = r,
                            Table = t,
                            Payment = p,
                            Details = from d in orderDetails
                                      join m in menus on d.MnuId equals m.MnuId
                                      where d.OrdId == o.OrdId && !d.IsDeleted
                                      select new
                                      {
                                          m.MnuName,
                                          d.OdtQuantity,
                                          m.MnuPrice
                                      }
                        };

            var data = query.FirstOrDefault();
            if (data == null) return null;

            decimal total = data.Details.Sum(x => x.MnuPrice * x.OdtQuantity);

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 36, 36, 54, 36);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Font Unicode
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                var fontTitle = new Font(bf, 16, Font.BOLD);
                var fontSubTitle = new Font(bf, 12, Font.BOLD);
                var fontNormal = new Font(bf, 11, Font.NORMAL);

                // ===== HEADER =====
                Paragraph title = new Paragraph("PIZZADAY", fontTitle) { Alignment = Element.ALIGN_CENTER };
                document.Add(title);

                Paragraph sub = new Paragraph("Địa chỉ: Vạn Phúc, Hà Đông, Hà Nội\nSố điện thoại: 123 456 789 | Email: pizzaday@restaurant.com", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 15f
                };
                document.Add(sub);

                // ===== THÔNG TIN HÓA ĐƠN =====
                PdfPTable infoTable = new PdfPTable(2) { WidthPercentage = 100 };
                infoTable.SetWidths(new float[] { 30, 70 });

                // Tạo mã hóa đơn ngắn gọn
                string invoiceCode = $"HD-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";

                infoTable.AddCell(new PdfPCell(new Phrase("Số hóa đơn:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(invoiceCode, fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.AddCell(new PdfPCell(new Phrase("Khách hàng:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(data.Reservation.TempCustomerName, fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.AddCell(new PdfPCell(new Phrase("Số điện thoại:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(data.Reservation.TempCustomerPhone, fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.AddCell(new PdfPCell(new Phrase("Ngày lập:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase($"{data.Reservation.ResDate:dd/MM/yyyy HH:mm}", fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.AddCell(new PdfPCell(new Phrase("Thời gian kết thúc:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(data.Reservation.ResEndTime?.ToString(@"hh\\:mm") ?? "N/A", fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.AddCell(new PdfPCell(new Phrase("Số người:", fontSubTitle)) { Border = Rectangle.NO_BORDER });
                infoTable.AddCell(new PdfPCell(new Phrase(data.Reservation.ResNumber.ToString(), fontNormal)) { Border = Rectangle.NO_BORDER });

                infoTable.SpacingAfter = 10f;
                document.Add(infoTable);

                // ===== BẢNG CHI TIẾT =====
                PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 10, 50, 20, 20 });

                string[] headers = { "STT", "Tên món", "Số lượng", "Thành tiền" };
                foreach (var h in headers)
                {
                    var cell = new PdfPCell(new Phrase(h, fontSubTitle))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        BackgroundColor = new BaseColor(230, 230, 230)
                    };
                    table.AddCell(cell);
                }

                int stt = 1;
                foreach (var item in data.Details)
                {
                    table.AddCell(new Phrase(stt.ToString(), fontNormal));
                    table.AddCell(new Phrase(item.MnuName, fontNormal));
                    table.AddCell(new Phrase(item.OdtQuantity.ToString(), fontNormal));
                    table.AddCell(new Phrase($"{item.MnuPrice * item.OdtQuantity:N0} VNĐ", fontNormal));
                    stt++;
                }

                if (!data.Details.Any())
                {
                    PdfPCell noDataCell = new PdfPCell(new Phrase("Không có món ăn", fontNormal))
                    {
                        Colspan = 4,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(noDataCell);
                }

                document.Add(table);

                // ===== TỔNG CỘNG =====
                Paragraph totalPara = new Paragraph($"Tổng cộng: {total:N0} VNĐ\nPhương thức thanh toán: {data.Payment.PayMethod}", fontSubTitle)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 15f
                };
                document.Add(totalPara);

                // ===== FOOTER =====
                Paragraph footer = new Paragraph("Cảm ơn quý khách - Hẹn gặp lại!", fontNormal)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 20f
                };
                document.Add(footer);

                document.Close();
                return ms.ToArray();
            }
        }

    }
}