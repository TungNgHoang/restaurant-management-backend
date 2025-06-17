using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Core.ApiModels;
using RestaurantManagement.Core.Enums;
using RestaurantManagement.Service.ApiModels;
using RestaurantManagement.Service.Dtos.MenusDto;
using RestaurantManagement.Service.Dtos.PromotionDto;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Service.Interfaces;
using Twilio.Jwt.Taskrouter;

namespace RestaurantManagement.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : BaseApiController
    {
        private readonly IPromotionService _promotionService;
        public PromotionController(IServiceProvider serviceProvider, IPromotionService promotionService) : base(serviceProvider)
        {
            _promotionService = promotionService;
        }

        [Authorize(Policy = "AdminManagerUserPolicy")]
        [HttpPost("get-all-promotion")]
        public async Task<IActionResult> GetAllPromotions([FromBody] PromotionModels pagingModel)
        {
            var promotions = await _promotionService.GetAllPromotionsAsync(pagingModel);
            var result = new PaginatedList<PromotionDto>(promotions.ToList(), promotions.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }

        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(Guid id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpPost("Add-promotion")]
        public async Task<IActionResult> AddPromotion([FromBody] PromotionDto promotionDto)
        {
            if (promotionDto == null)
                return BadRequest(StatusCodeEnum.BadRequest);
            var newPromotion = await _promotionService.AddPromotionAsync(promotionDto);
            return Ok(newPromotion);
        }

        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] PromotionDto promotionDto)
        {
            var updatedPromotion = await _promotionService.UpdatePromotionAsync(id, promotionDto);
            if (updatedPromotion == null) return NotFound();
            return Ok(updatedPromotion);
        }

        [Authorize(Policy = "AdminManagerPolicy")]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (!result) return NotFound(new { message = StatusCodeEnum.D04 });
            return Ok(new { message = StatusCodeEnum.D05 });

        }
    }
}