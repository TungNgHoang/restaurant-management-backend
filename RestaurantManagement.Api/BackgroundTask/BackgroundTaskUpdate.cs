namespace RestaurantManagement.Api.BackgroundTask
{
    public class BackgroundTaskUpdate(IServiceProvider serviceProvider,
                              ILogger<BackgroundTaskUpdate> logger,
                              IConfiguration configuration) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<BackgroundTaskUpdate> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTaskUpdate service đang hoạt động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredReservations(stoppingToken);
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
                    .FilterAsync(r => r.ResAutoCancelAt != null && r.ResAutoCancelAt < nowGmt7 && r.IsDeleted == false);

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
                        await ProcessSingleReservation(reservation, orderRepo, orderDetailRepo, stoppingToken);
                        processedCount++;

                        _logger.LogDebug("Đã xử lý đặt bàn: {ReservationId}", reservation.ResId);
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
