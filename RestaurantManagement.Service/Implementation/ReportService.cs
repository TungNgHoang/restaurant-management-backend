namespace RestaurantManagement.Service.Implementation
{
    public class ReportService : BaseService, IReportService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly RestaurantDBContext _dbContext;
        private readonly IRepository<TblOrderDetail> _orderDetailRepository;
        private readonly IRepository<TblReservation> _reservationRepository;
        private readonly IRepository<TblCustomer> _customerRepository;
        private readonly IRepository<TblPayment> _paymentRepository;

        public ReportService(
            AppSettings appSettings,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            RestaurantDBContext dbContext,
            IRepository<TblPayment> paymentRepository,
            IRepository<TblOrderDetail> orderDetailRepository,
            IRepository<TblReservation> reservationRepository,
            IRepository<TblCustomer> customerRepository
            ) : base(appSettings, mapper, httpContextAccessor)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _paymentRepository = paymentRepository;
            _orderDetailRepository = orderDetailRepository;
            _reservationRepository = reservationRepository;
            _customerRepository = customerRepository;
            _dbContext = dbContext;
        }

        public async Task<List<ReportDto>> GetAllReportsAsync(ReportModels model)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(model);

            // Lấy danh sách các ngày có giao dịch
            var paymentDates = await _dbContext.TblPayments
                .Select(p => p.CreatedAt.Date)
                .Distinct()
                .OrderBy(date => date)
                .ToListAsync();

            //tạo danh sách báo cáo
            var reportList = new List<ReportDto>();

            foreach (var date in paymentDates)
            {
                var nextDate = date.AddDays(1);

                var totalRevenue = await _dbContext.TblPayments
                    .Where(p => p.CreatedAt >= date && p.CreatedAt < nextDate)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                var totalDishesSold = await _dbContext.TblOrderDetails
                    .Where(od => od.CreatedAt >= date && od.CreatedAt < nextDate)
                    .SumAsync(od => (int?)od.OdtQuantity) ?? 0;

                var totalReservations = await _dbContext.TblReservations
                    .Where(r => r.ResDate >= date && r.ResDate < nextDate)
                    .CountAsync();

                var totalCustomers = await _dbContext.TblCustomers
                    .Where(c => c.CreatedAt >= date && c.CreatedAt < nextDate)
                    .CountAsync();

                var bestSellingDish = await _dbContext.TblOrderDetails
                    .Where(od => od.CreatedAt >= date && od.CreatedAt < nextDate)
                    .GroupBy(od => od.MnuId)
                    .Select(g => new
                    {
                        MenuId = g.Key,
                        TotalQuantity = g.Sum(x => x.OdtQuantity)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Join(_dbContext.TblMenus, g => g.MenuId, m => m.MnuId, (g, m) => new { m.MnuName })
                    .Select(x => x.MnuName)
                    .FirstOrDefaultAsync() ?? "Không có dữ liệu";

                reportList.Add(new ReportDto
                {
                    ReportDay = date.Day.ToString("00"),
                    ReportMonth = date.Month.ToString("00"),
                    ReportYear = date.Year.ToString(),
                    TotalRevenue = totalRevenue,
                    TotalDishesSold = totalDishesSold,
                    TotalReservations = totalReservations,
                    TotalCustomers = totalCustomers,
                    BestSellingDish = bestSellingDish
                });
            }

            var reportDtos = _mapper.Map<List<ReportDto>>(reportList);
            var result = AdvancedFilter(reportDtos.AsEnumerable(), model, nameof(ReportDto.ReportDay)).ToList();
            return result;

        }

        private void ValidatePagingModel(ReportModels pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }
    }
}
