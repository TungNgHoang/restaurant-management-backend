namespace RestaurantManagement.Api.BackgroundTask
{
    public class BackgroundTaskUpdate(IServiceProvider serviceProvider,
                              ILogger<BackgroundTaskUpdate> logger,
                              IConfiguration configuration) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<BackgroundTaskUpdate> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private DateTime _lastMonthlyReportDate = DateTime.MinValue;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTaskUpdate service đang hoạt động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Xử lý các tác vụ định kỳ

                    //Xử lý các đơn đặt đã quá hạn
                    await ProcessExpiredReservations(stoppingToken);
                    //Xử lý các đơn đặt sắp quá hạn
                    await ProcessExpiringReservations(stoppingToken);
                    //Xử lý báo cáo doanh thu hàng tháng
                    await ProcessMonthlyRevenueReport(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Lỗi khi xử lý đặt bàn quá hạn");
                    // Tiếp tục chạy thay vì crash toàn bộ service
                }

                var intervalMinutes = _configuration.GetValue<int>("BackgroundTasks:CleanupIntervalMinutes", 1);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }

            _logger.LogInformation("BackgroundTaskUpdate service dừng");
        }

        private async Task ProcessExpiredReservations(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reservationRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblReservation>>();
            var orderRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblOrderInfo>>();
            var orderDetailRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblOrderDetail>>();
            var tableRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblTableInfo>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Sử dụng timezone từ configuration
            var timeZone = _configuration.GetValue<string>("Application:TimeZone", "SE Asia Standard Time");
            if (string.IsNullOrWhiteSpace(timeZone))
            {
                ArgumentException argumentException = new("The configured time zone is null or empty.", nameof(timeZone));
                throw argumentException;
            }
            var nowGmt7 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                                        TimeZoneInfo.FindSystemTimeZoneById(timeZone));

            _logger.LogDebug("Processing expired reservations at {Time}", nowGmt7);

            try
            {
                // Query expired reservations
                var expiredReservations = await reservationRepo
                    .FilterAsync(r => r.ResAutoCancelAt != null &&
                                     r.ResAutoCancelAt < nowGmt7 &&
                                     r.IsDeleted == false &&
                                     r.ResStatus == ReservationStatus.Pending.ToString());

                if (!expiredReservations.Any())
                {
                    _logger.LogDebug("Không có đặt bàn nào quá hạn");
                    return;
                }

                _logger.LogInformation("Tìm thấy {Count} đặt bàn quá hạn", expiredReservations.Count());

                int processedCount = 0;
                int errorCount = 0;

                // Process each reservation in separate transaction
                foreach (var reservation in expiredReservations)
                {
                    try
                    {
                        // Kiểm tra có đơn đặt trước không
                        var preOrders = await orderRepo.FilterAsync(o => o.ResId == reservation.ResId &&
                            o.OrdStatus == OrderStatusEnum.PreOrder.ToString() && !o.IsDeleted);
                        bool hasPreOrder = preOrders.Any();

                        var customerName = reservation.TempCustomerName ?? "Khách hàng";

                        // Xử lý hủy đặt bàn
                        await ProcessSingleReservation(reservation, orderRepo, orderDetailRepo, stoppingToken);

                        // Gửi thông báo hủy tự động
                        await notificationService.SendReservationAutoCancelledNotificationAsync(
                            reservation.ResId, customerName, hasPreOrder);

                        processedCount++;
                        _logger.LogDebug("Đã xử lý và gửi thông báo hủy đặt bàn: {ReservationId}", reservation.ResId);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, "Không thể xử lý đặt bàn {ReservationId}", reservation.ResId);
                    }
                }

                _logger.LogInformation("Hoàn thành xử lý: {ProcessedCount} thành công, {ErrorCount} lỗi",
                                     processedCount, errorCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi truy vấn đặt bàn quá hạn");
                throw;
            }
        }

        private async Task ProcessExpiringReservations(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var reservationRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblReservation>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var timeZone = _configuration.GetValue<string>("Application:TimeZone", "SE Asia Standard Time");
            var nowGmt7 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                                        TimeZoneInfo.FindSystemTimeZoneById(timeZone));
            var warningTime = nowGmt7.AddMinutes(5); // 5 minutes from now

            try
            {
                // Query reservations expiring in 5 minutes
                var expiringReservations = await reservationRepo
                    .FilterAsync(r => r.ResAutoCancelAt != null &&
                                     r.ResAutoCancelAt <= warningTime &&
                                     r.ResAutoCancelAt > nowGmt7 &&
                                     r.IsDeleted == false &&
                                     r.ResStatus == ReservationStatus.Pending.ToString());

                if (!expiringReservations.Any())
                {
                    _logger.LogDebug("Không có đặt bàn nào sắp quá hạn");
                    return;
                }

                _logger.LogInformation("Tìm thấy {Count} đặt bàn sắp quá hạn", expiringReservations.Count());

                foreach (var reservation in expiringReservations)
                {
                    try
                    {
                        var customerName = reservation.TempCustomerName ?? "Khách hàng";
                        await notificationService.SendReservationExpiringSoonNotificationAsync(
                            reservation.ResId, customerName, reservation.ResAutoCancelAt.Value);

                        _logger.LogDebug("Đã gửi thông báo sắp quá hạn cho đặt bàn: {ReservationId}", reservation.ResId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi gửi thông báo sắp quá hạn cho đặt bàn {ReservationId}", reservation.ResId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thông báo đặt bàn sắp quá hạn");
            }
        }

        private async Task ProcessMonthlyRevenueReport(CancellationToken stoppingToken)
        {
            var timeZone = _configuration.GetValue<string>("Application:TimeZone", "SE Asia Standard Time");
            var nowGmt7 = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                                                        TimeZoneInfo.FindSystemTimeZoneById(timeZone));

            // Check if we need to send monthly report (first day of month and haven't sent this month)
            if (nowGmt7.Day != 1 || _lastMonthlyReportDate.Month == nowGmt7.Month && _lastMonthlyReportDate.Year == nowGmt7.Year)
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var paymentRepo = scope.ServiceProvider.GetRequiredService<IRepository<TblPayment>>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var lastMonth = nowGmt7.AddMonths(-1);
                var firstDayOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                var lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddDays(-1);

                // Calculate total revenue for last month
                var monthlyPayments = await paymentRepo.FilterAsync(p =>
                    p.CreatedAt >= firstDayOfLastMonth &&
                    p.CreatedAt <= lastDayOfLastMonth &&
                    p.PayStatus == "Completed" &&
                    !p.IsDeleted);

                var totalRevenue = monthlyPayments.Sum(p => p.Amount);

                await notificationService.SendMonthlyRevenueReportNotificationAsync(
                    totalRevenue, lastMonth.Month, lastMonth.Year);

                _lastMonthlyReportDate = nowGmt7;
                _logger.LogInformation("Đã gửi báo cáo doanh thu tháng {Month}/{Year}: {Revenue:C0} VND",
                    lastMonth.Month, lastMonth.Year, totalRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo báo cáo doanh thu hàng tháng");
            }
        }

        private async Task ProcessSingleReservation(TblReservation reservation,
                                                  IRepository<TblOrderInfo> orderRepo,
                                                  IRepository<TblOrderDetail> orderDetailRepo,
                                                  CancellationToken stoppingToken)
        {
            // Sử dụng transaction scope để đảm bảo consistency
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDBContext>();

            using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

            try
            {

                var orders = await orderRepo.FilterAsync(o => o.ResId == reservation.ResId);
                var orderIds = orders.Select(o => o.OrdId).ToList();
                var orderDetails = await orderDetailRepo.FilterAsync(od => orderIds.Contains(od.OrdId));
                // Delete related order details first
                foreach (var orderDetail in orderDetails)
                {
                    await orderDetailRepo.DeleteAsync(orderDetail);
                }
                // Delete related orders first
                foreach (var order in orders)
                {
                    await orderRepo.DeleteAsync(order);
                }

                // Delete the reservation itself
                await scope.ServiceProvider.GetRequiredService<IRepository<TblReservation>>()
                    .DeleteAsync(reservation);


                await transaction.CommitAsync(stoppingToken);

                _logger.LogInformation("Successfully deleted expired reservation {ReservationId} and {OrderCount} orders and {OrderDetailCount} order details",
                                     reservation.ResId, orders.Count(), orderDetails.Count());
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(stoppingToken);
                throw;
            }
        }
    }
}
