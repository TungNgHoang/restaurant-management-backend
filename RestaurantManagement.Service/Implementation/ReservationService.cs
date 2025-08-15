using Microsoft.AspNetCore.Http.HttpResults;
using RestaurantManagement.DataAccess.Interfaces;

namespace RestaurantManagement.Service.Implementation
{
    public class ReservationService : BaseService, IReservationService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly RestaurantDBContext _dbContext;
        private readonly IRepository<TblCustomer> _customerRepository;
        private readonly IRepository<TblReservation> _reservationsRepository;
        private readonly IRepository<TblTableInfo> _tablesRepository;
        private readonly IRepository<TblOrderInfo> _ordersRepository;
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(
            IHttpContextAccessor httpContextAccessor,
            AppSettings appSettings,
            RestaurantDBContext dbContext,
            IMapper mapper,
            IRepository<TblReservation> reservationsRepository,
            IRepository<TblTableInfo> tablesRepository,
            IRepository<TblCustomer> customerRepository,
            IRepository<TblOrderInfo> ordersRepository,
            IReservationRepository reservationRepository
            ) : base(appSettings, mapper, httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _reservationsRepository = reservationsRepository;
            _tablesRepository = tablesRepository;
            _customerRepository = customerRepository;
            _reservationRepository = reservationRepository;
            _mapper = mapper;
            _dbContext = dbContext;
            _ordersRepository = ordersRepository;
        }

        public async Task<List<AvailableTableDto>> GetAvailableTablesAsync(CheckAvailabilityRequestDto request)
        {
            var startTime = request.ResDate;
            var endTime = request.ResEndDate;
            var capacity = request.ResNumber;


            var allTables = await _tablesRepository.ActiveRecordsAsync();
            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(startTime, endTime);
            var occupiedTableIds = overlappingReservations.Select(r => r.TbiId).Distinct().ToList();

            var availableTables = allTables
                .Where(t => !occupiedTableIds.Contains(t.TbiId) && t.TbiCapacity >= capacity)
                .ToList();

            return _mapper.Map<List<AvailableTableDto>>(availableTables);
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationRequestDto request)
        {
            var startTime = request.ResDate;
            var endTime = request.ResEndTime;
            Guid? createdBy = null;

            if (startTime > endTime)
                throw new ErrorException(StatusCodeEnum.C02);
            if (!string.IsNullOrEmpty(request.TempCustomerPhone) && request.TempCustomerPhone.Length != 10)
                throw new ErrorException(StatusCodeEnum.C03);

            var overlappingReservations = await _reservationRepository.GetOverlappingReservationsAsync(startTime, endTime);
            if (overlappingReservations.Any(r => r.TbiId == request.TbiId))
            {
                throw new ErrorException(StatusCodeEnum.C01);
            }

            var table = await _tablesRepository.FindByIdAsync(request.TbiId);
            if (table == null || request.ResNumber > table.TbiCapacity)
                throw new ErrorException(StatusCodeEnum.A02);

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                createdBy = Guid.Parse(userIdClaim);
            }
            var reservation = new TblReservation
            {
                ResId = Guid.NewGuid(),
                TbiId = request.TbiId,
                TempCustomerName = request.TempCustomerName,
                TempCustomerPhone = request.TempCustomerPhone,
                TempCustomerMail = request.TempCustomerEmail,
                ResDate = request.ResDate,
                ResEndTime = request.ResEndTime,
                ResNumber = request.ResNumber,
                ResStatus = ReservationStatus.Pending.ToString(),
                Note = request.Note,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy ?? Guid.Empty
            };

            await _reservationsRepository.InsertAsync(reservation);
            return _mapper.Map<ReservationResponseDto>(reservation);
        }

        public async Task<IEnumerable<ReserDto>> GetAllReservationsAsync(ReserModel pagingModel)
        {
            // Validate PageIndex and PageSize
            ValidatePagingModel(pagingModel);

            var reservations = await _reservationsRepository.AsNoTrackingAsync();
            var tables = await _tablesRepository.AsNoTrackingAsync();
            var orders = await _ordersRepository.AsNoTrackingAsync();

            // Join Reservation và Table
            var data = from reservation in reservations
                       join table in tables on reservation.TbiId equals table.TbiId
                       join order in orders on reservation.ResId equals order.ResId into orderGroup
                       from order in orderGroup.DefaultIfEmpty() // Left join
                       select new
                       {
                           reservation.ResId,
                           OrdId = order?.OrdId,
                           reservation.TempCustomerName,
                           reservation.TempCustomerPhone,
                           reservation.TempCustomerMail,
                           reservation.ResDate,
                           reservation.ResEndTime,
                           reservation.ResStatus,
                           reservation.ResNumber,
                           table.TbiTableNumber
                       };

            // Ánh xạ sang ReserDto với tách ngày và giờ
            var reserDto = data.Select(x => new ReserDto
            {
                ResId = x.ResId,
                OrdId = x.OrdId,
                TableNumber = x.TbiTableNumber,
                CustomerName = x.TempCustomerName,
                ContactPhone = x.TempCustomerPhone,
                ReservationDate = x.ResDate.Date,
                TimeIn = x.ResDate.TimeOfDay,
                TimeOut = x.ResEndTime?.TimeOfDay ?? TimeSpan.Zero,
                NumberOfPeople = x.ResNumber,
                Status = x.ResStatus
            }).ToList();
            // Apply search filter on the DTOs
            var result = AdvancedFilter(reserDto.AsEnumerable(), pagingModel, nameof(ReserDto.ReservationDate));

            return result;
        }

        public async Task CheckInReservationAsync(Guid resId, int actualNumber)
        {
            // Sử dụng transaction
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Tìm reservation theo ResID
                    var reservation = await _reservationsRepository.FindByIdAsync(resId);
                    if (reservation == null || reservation.IsDeleted)
                        throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

                    // Kiểm tra trạng thái reservation phải là "Pending"
                    if (reservation.ResStatus != ReservationStatus.Pending.ToString())
                        throw new ErrorException(StatusCodeEnum.A01);

                    var customer = await _customerRepository.FindAsync(
                        c => c.CusEmail == reservation.TempCustomerMail && !c.IsDeleted);

                    Guid? createdBy = null;
                    var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim))
                    {
                        createdBy = Guid.Parse(userIdClaim);
                    }
                    if (customer == null)
                    {
                        customer = new TblCustomer
                        {
                            CusId = Guid.NewGuid(),
                            CusName = reservation.TempCustomerName,
                            CusContact = reservation.TempCustomerPhone,
                            CusEmail = reservation.TempCustomerMail,
                            IsDeleted = false,
                            CreatedAt = DateTime.Now,
                            CreatedBy = createdBy ?? Guid.Empty // Giả định tạm thời
                        };
                        await _customerRepository.InsertAsync(customer);
                    }
                    else
                    {
                        // Cập nhật thông tin khách hàng nếu cần thiết
                        customer.CusName = reservation.TempCustomerName;
                        customer.CusContact = reservation.TempCustomerPhone;
                        customer.UpdatedAt = DateTime.Now;
                        customer.UpdatedBy = createdBy ??  Guid.Empty; // Giả định tạm thời
                        await _customerRepository.UpdateAsync(customer);
                        reservation.ResActualNumber = actualNumber;

                        
                    }

                    // Cập nhật trạng thái reservation thành "Serving"
                    reservation.ResStatus = ReservationStatus.Serving.ToString();
                    reservation.UpdatedAt = DateTime.Now;
                    reservation.UpdatedBy = createdBy ?? Guid.Empty; // Giả định tạm thời
                    reservation.CusId = customer.CusId; // Gán CusId vào reservation
                    // Lấy thông tin bàn
                    var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
                    if (table == null)
                        throw new ErrorException(StatusCodeEnum.C04);

                    // Kiểm tra trạng thái bàn phải là "Empty"
                    if (table.TbiStatus != TableStatus.Empty.ToString())
                        throw new ErrorException(StatusCodeEnum.A02);

                    // Kiểm tra số người thực tế không vượt quá sức chứa của bàn
                    if (actualNumber > table.TbiCapacity)
                        throw new ErrorException(StatusCodeEnum.C09);

                    // Cập nhật trạng thái bàn thành "Occupied"
                    table.TbiStatus = TableStatus.Occupied.ToString();
                    table.UpdatedAt = DateTime.Now;
                    table.UpdatedBy = createdBy ?? Guid.Empty; // Giả định tạm thời

                    // Tìm đơn đặt món trước (nếu có) để cập nhật trạng thái
                    var preOrder = await _ordersRepository.FindAsync(
                        o => o.ResId == reservation.ResId && o.OrdStatus == OrderStatusEnum.PreOrder.ToString() && !o.IsDeleted);

                    if (preOrder != null)
                    {
                        preOrder.OrdStatus = OrderStatusEnum.Order.ToString();
                        preOrder.UpdatedAt = DateTime.Now;
                        preOrder.UpdatedBy = createdBy ?? Guid.Empty;
                        await _ordersRepository.UpdateAsync(preOrder);
                    }

                    // Lưu thay đổi vào database
                    await _reservationsRepository.UpdateAsync(reservation);
                    await _tablesRepository.UpdateAsync(table);

                    // Commit transaction nếu mọi thứ thành công
                    await transaction.CommitAsync();  
                }

                catch (ErrorException ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new ErrorException($"Lỗi khi check-in: {ex.Message}");
                }
            }
        }

        private void ValidatePagingModel(ReserModel pagingModel)
        {
            if (pagingModel.PageIndex < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageIndexInvalid);
            if (pagingModel.PageSize < 1)
                throw new ErrorException(Core.Enums.StatusCodeEnum.PageSizeInvalid);
        }

        public async Task<ReserDto> GetReservationByIdAsync(Guid resId)
        {
            // Tìm reservation theo resid
            var reservation = await _reservationsRepository.FindByIdAsync(resId);
            if (reservation == null)
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

            // Lấy thông tin bàn
            var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
            if (table == null)
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

            // Tạo đối tượng ReserDto
            var reserDto = new ReserDto
            {
                ResId = reservation.ResId,
                TableNumber = table.TbiTableNumber,
                CustomerName = reservation.TempCustomerName,
                ContactPhone = reservation.TempCustomerPhone,
                ReservationDate = reservation.ResDate.Date,
                TimeIn = reservation.ResDate.TimeOfDay,
                TimeOut = reservation.ResEndTime?.TimeOfDay ?? TimeSpan.Zero,
                NumberOfPeople = reservation.ResNumber,
                Status = reservation.ResStatus
            };

            // Nếu có CusId, lấy thông tin khách hàng từ TblCustomer
            if (reservation.CusId.HasValue)
            {
                var customer = await _customerRepository.FindByIdAsync(reservation.CusId.Value);
                if (customer != null)
                {
                    reserDto.CustomerName = customer.CusName;
                    reserDto.ContactPhone = customer.CusContact;
                }
            }

            return reserDto;
        }

        public async Task CancelReservationAsync(Guid resId)
        {
            // Tìm reservation theo ID
            var reservation = await _reservationsRepository.FindByIdAsync(resId);
            if (reservation == null)
                throw new ErrorException(StatusCodeEnum.ReservatioNotFound);

            // Kiểm tra trạng thái phải là "Pending" hoặc "Serving"
            if (reservation.ResStatus == ReservationStatus.Cancelled.ToString())
                throw new ErrorException(StatusCodeEnum.A03);

            // Cập nhật trạng thái thành "Cancelled"
            reservation.ResStatus = ReservationStatus.Cancelled.ToString();
            reservation.UpdatedAt = DateTime.Now;
            reservation.UpdatedBy = Guid.Empty; // Giả định tạm thời

            // Kiểm tra nếu bàn đang bị chiếm dụng thì chuyển về trạng thái trống
            var table = await _tablesRepository.FindByIdAsync(reservation.TbiId);
            if (table != null && table.TbiStatus == TableStatus.Occupied.ToString())
            {
                table.TbiStatus = TableStatus.Empty.ToString();
                table.UpdatedAt = DateTime.Now;
                table.UpdatedBy = Guid.Empty;
                await _tablesRepository.UpdateAsync(table);
            }

            // Lưu thay đổi vào database
            await _reservationsRepository.UpdateAsync(reservation);
        }
    }
}
