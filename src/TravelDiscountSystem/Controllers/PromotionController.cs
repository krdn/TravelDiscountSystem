using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelDiscountSystem.Domain;
using TravelDiscountSystem.Data;

namespace TravelDiscountSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<PromotionController> _logger;

        public PromotionController(IPromotionRepository promotionRepository, ILogger<PromotionController> logger)
        {
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        /// <summary>
        /// 활성 프로모션 목록 조회
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Promotion>>> GetActivePromotions()
        {
            try
            {
                var promotions = await _promotionRepository.GetActivePromotionsAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "프로모션 목록 조회 중 오류 발생");
                return StatusCode(500, new { message = "프로모션 목록 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 프로모션 상세 조회
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Promotion>> GetPromotion(int id)
        {
            try
            {
                var promotion = await _promotionRepository.GetByIdAsync(id);
                if (promotion == null)
                {
                    return NotFound();
                }
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "프로모션 상세 조회 중 오류 발생");
                return StatusCode(500, new { message = "프로모션 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 프로모션 생성
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Promotion>> CreatePromotion([FromBody] Promotion promotion)
        {
            try
            {
                var createdPromotion = await _promotionRepository.CreateAsync(promotion);
                return CreatedAtAction(nameof(GetPromotion), new { id = createdPromotion.Id }, createdPromotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "프로모션 생성 중 오류 발생");
                return StatusCode(500, new { message = "프로모션 생성 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 프로모션 수정
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Promotion>> UpdatePromotion(int id, [FromBody] Promotion promotion)
        {
            try
            {
                if (id != promotion.Id)
                {
                    return BadRequest();
                }

                var updatedPromotion = await _promotionRepository.UpdateAsync(promotion);
                return Ok(updatedPromotion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "프로모션 수정 중 오류 발생");
                return StatusCode(500, new { message = "프로모션 수정 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 프로모션 삭제
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePromotion(int id)
        {
            try
            {
                var result = await _promotionRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "프로모션 삭제 중 오류 발생");
                return StatusCode(500, new { message = "프로모션 삭제 중 오류가 발생했습니다." });
            }
        }
    }
}