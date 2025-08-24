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

        [Authorize(Policy = "SaMPolicy")]
        [HttpPost("get-all-promotion")]
        public async Task<IActionResult> GetAllPromotions([FromBody] PromotionModels pagingModel)
        {
            var promotions = await _promotionService.GetAllPromotionsAsync(pagingModel);
            var result = new PaginatedList<PromotionDto>(promotions.ToList(), promotions.Count(), pagingModel.PageIndex, pagingModel.PageSize);
            return Ok(result);
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(Guid id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null)
                throw new ErrorException(StatusCodeEnum.D04);
            return Ok(promotion);
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpPost("Add-promotion")]
        public async Task<IActionResult> AddPromotion([FromBody] PromotionDto promotionDto)
        {
            if (promotionDto == null)
                return BadRequest(StatusCodeEnum.D04);
            var newPromotion = await _promotionService.AddPromotionAsync(promotionDto);
            return Ok(newPromotion);
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdatePromotion(Guid id, [FromBody] PromotionDto promotionDto)
        {
            var updatedPromotion = await _promotionService.UpdatePromotionAsync(id, promotionDto);
            if (updatedPromotion == null) throw new ErrorException(StatusCodeEnum.D04);
            return Ok(updatedPromotion);
        }

        [Authorize(Policy = "ManagerPolicy")]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePromotion(Guid id)
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (!result) throw new ErrorException(StatusCodeEnum.D04);
            return Ok(new { message = StatusCodeEnum.D05 });

        }

        [Authorize(Policy = "BillingPolicy")]
        [HttpGet("available/{reservationId}")]
        public async Task<IActionResult> GetAvailablePromotions(Guid reservationId)
        {
            try
            {
                var promotions = await _promotionService.GetAvailablePromotionAsync(reservationId);
                return Ok(new
                {
                    Success = true,
                    Data = promotions
                });
            }
            catch (ErrorException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Code = ex.StatusCode,
                    Message = StatusCodeEnum.BadRequest
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}