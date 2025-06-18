using RestaurantManagement.Service.Dtos.PromotionDto;
using RestaurantManagement.Service.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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