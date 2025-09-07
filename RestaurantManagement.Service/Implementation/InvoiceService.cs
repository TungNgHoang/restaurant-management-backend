using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using RestaurantManagement.Service.Dtos.InvoiceDto;


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

        public async Task<byte[]> GenerateInvoicePdf(Guid orderId, InvoicePrintDto dto)
        {
            if (dto == null) return null;

            var vi = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A7);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // Header
                        col.Item().AlignCenter().Text(dto.StoreName).Bold().FontSize(14);
                        if (!string.IsNullOrWhiteSpace(dto.StoreAddress))
                            col.Item().AlignCenter().Text(dto.StoreAddress).FontSize(8);
                        if (!string.IsNullOrWhiteSpace(dto.StorePhone))
                            col.Item().AlignCenter().Text("Tel: " + dto.StorePhone).FontSize(8);

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Meta
                        col.Item().Grid(grid =>
                        {
                            grid.Columns(2);
                            grid.Item().Text("Số HĐ:").Bold();
                            grid.Item().Text(dto.InvoiceCode ?? "-");

                            grid.Item().Text("Ngày:").Bold();
                            grid.Item().Text(dto.InvoiceDate.ToString("dd/MM/yyyy HH:mm"));

                            grid.Item().Text("Khách:").Bold();
                            grid.Item().Text(dto.CustomerName ?? "-");

                            grid.Item().Text("Bàn:").Bold();
                            grid.Item().Text(dto.TableNumber ?? "-");
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Items
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20); // TT
                                columns.RelativeColumn(3);  // Tên
                                columns.ConstantColumn(25); // SL
                                columns.RelativeColumn(2);  // Thành tiền
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Text("TT").Bold().AlignCenter();
                                header.Cell().Text("Món").Bold();
                                header.Cell().Text("SL").Bold().AlignCenter();
                                header.Cell().Text("Thành tiền").Bold().AlignRight();
                            });

                            if (dto.Items != null && dto.Items.Any())
                            {
                                foreach (var it in dto.Items)
                                {
                                    table.Cell().Text(it.Index.ToString()).AlignCenter();
                                    table.Cell().Text(it.Name);
                                    table.Cell().Text(it.Quantity.ToString()).AlignCenter();
                                    table.Cell().Text(string.Format(vi, "{0:N0}₫", it.LineTotal)).AlignRight();
                                }
                            }
                            else
                            {
                                table.Cell().ColumnSpan(4).AlignCenter().Text("Không có món");
                            }
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Black);

                        // Totals
                        col.Item().PaddingTop(4).Column(tot =>
                        {
                            void AddRow(string label, string value, bool bold = false)
                            {
                                tot.Item().Row(r =>
                                {
                                    var textLabel = r.RelativeItem().Text(label);
                                    if (bold) textLabel.Bold();

                                    var textValue = r.ConstantItem(70).AlignRight().Text(value);
                                    if (bold) textValue.Bold();
                                });
                            }

                            AddRow("Tạm tính:", string.Format(vi, "{0:N0}₫", dto.SubTotal));
                            AddRow($"VAT ({dto.VatRate:P0}):", string.Format(vi, "{0:N0}₫", dto.VatAmount));

                            if (!string.IsNullOrWhiteSpace(dto.VoucherCode) && dto.VoucherDiscount > 0)
                                AddRow($"Voucher ({dto.VoucherCode}):", "-" + string.Format(vi, "{0:N0}₫", dto.VoucherDiscount));

                            if (dto.RankDiscount > 0)
                                AddRow("Giảm hạng:", "-" + string.Format(vi, "{0:N0}₫", dto.RankDiscount));

                            AddRow("TỔNG:", string.Format(vi, "{0:N0}₫", dto.TotalAmount), bold: true);
                            AddRow("Thanh toán:", dto.PayMethod ?? "-", bold: false);
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Footer
                        col.Item().AlignCenter().Text("⭐ Cảm ơn quý khách! Hẹn gặp lại ⭐")
                            .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}