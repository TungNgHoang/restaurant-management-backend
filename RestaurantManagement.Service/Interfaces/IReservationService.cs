namespace RestaurantManagement.Service.Interfaces
{
    public interface IReservationService
    {
        Task<List<AvailableTableDto>> GetAvailableTablesAsync(CheckAvailabilityRequestDto request);
        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationRequestDto request);
        Task<IEnumerable<ReserDto>> GetAllReservationsAsync(ReserModel pagingModel);
        Task CheckInReservationAsync(Guid resId, int actualNumber);
        Task<ReserDto> GetReservationByIdAsync(Guid resId);
        Task CancelReservationAsync(Guid resId);
        Task<ReserDto> UpdateReservationAsync(Guid resId, UpdateReservationRequestDto request);
    }
}
