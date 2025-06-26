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
    public class DiscountConditionController : ControllerBase
    {
        private readonly IDiscountConditionRepository _discountConditionRepository;
        private readonly ILogger<DiscountConditionController> _logger;

        public DiscountConditionController(IDiscountConditionRepository discountConditionRepository, ILogger<DiscountConditionController> logger)
        {
            _discountConditionRepository = discountConditionRepository;
            _logger = logger;
        }

        /// <summary>
        /// 활성 할인조건 목록 조회
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DiscountCondition>>> GetActiveConditions()
        {
            try
            {
                var conditions = await _discountConditionRepository.GetActiveConditionsAsync();
                return Ok(conditions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 목록 조회 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 목록 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 할인조건 상세 조회
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscountCondition>> GetCondition(int id)
        {
            try
            {
                var condition = await _discountConditionRepository.GetByIdAsync(id);
                if (condition == null)
                {
                    return NotFound();
                }
                return Ok(condition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 상세 조회 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 할인조건 생성
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DiscountCondition>> CreateCondition([FromBody] DiscountCondition condition)
        {
            try
            {
                var createdCondition = await _discountConditionRepository.CreateAsync(condition);
                return CreatedAtAction(nameof(GetCondition), new { id = createdCondition.Id }, createdCondition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 생성 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 생성 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 할인조건 수정
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<DiscountCondition>> UpdateCondition(int id, [FromBody] DiscountCondition condition)
        {
            try
            {
                if (id != condition.Id)
                {
                    return BadRequest();
                }

                var updatedCondition = await _discountConditionRepository.UpdateAsync(condition);
                return Ok(updatedCondition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 수정 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 수정 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 할인조건 삭제
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCondition(int id)
        {
            try
            {
                var result = await _discountConditionRepository.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 삭제 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 삭제 중 오류가 발생했습니다." });
            }
        }
    }
}