namespace RestaurantManagement.Service.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<PromotionDto>> GetAllPromotionsAsync(PromotionModels pagingModel);
        Task<PromotionDto> GetPromotionByIdAsync(Guid id);
        Task<PromotionDto> AddPromotionAsync(PromotionDto promotionDto);
        Task<PromotionDto> UpdatePromotionAsync(Guid id, PromotionDto promotionDto);
        Task<bool> DeletePromotionAsync(Guid id);
    }
}