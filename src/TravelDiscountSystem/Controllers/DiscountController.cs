using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelDiscountSystem.Domain;
using TravelDiscountSystem.Services;
using TravelDiscountSystem.Data;

namespace TravelDiscountSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(IDiscountService discountService, ILogger<DiscountController> logger)
        {
            _discountService = discountService;
            _logger = logger;
        }

        /// <summary>
        /// 할인 금액 계산
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<DiscountCalculationResult>> CalculateDiscount([FromBody] DiscountCalculationRequest request)
        {
            try
            {
                var result = await _discountService.CalculateDiscountAsync(
                    request.BookingInfo, 
                    request.DiscountConditionIds, 
                    request.CouponCodes);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인 계산 중 오류 발생");
                return StatusCode(500, new { message = "할인 계산 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 적용 가능한 할인조건 조회
        /// </summary>
        [HttpPost("conditions/applicable")]
        public async Task<ActionResult<List<DiscountCondition>>> GetApplicableConditions([FromBody] BookingInfo bookingInfo)
        {
            try
            {
                var conditions = await _discountService.GetApplicableDiscountConditionsAsync(bookingInfo);
                return Ok(conditions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "적용 가능한 할인조건 조회 중 오류 발생");
                return StatusCode(500, new { message = "할인조건 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 적용 가능한 쿠폰 조회
        /// </summary>
        [HttpPost("coupons/applicable")]
        public async Task<ActionResult<List<DiscountCoupon>>> GetApplicableCoupons([FromBody] BookingInfo bookingInfo)
        {
            try
            {
                var coupons = await _discountService.GetApplicableCouponsAsync(bookingInfo);
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "적용 가능한 쿠폰 조회 중 오류 발생");
                return StatusCode(500, new { message = "쿠폰 조회 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 할인조건 유효성 검증
        /// </summary>
        [HttpPost("conditions/{conditionId}/validate")]
        public async Task<ActionResult<DiscountValidationResult>> ValidateCondition(int conditionId, [FromBody] BookingInfo bookingInfo)
        {
            try
            {
                var result = await _discountService.ValidateDiscountConditionAsync(conditionId, bookingInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인조건 유효성 검증 중 오류 발생");
                return StatusCode(500, new { message = "유효성 검증 중 오류가 발생했습니다." });
            }
        }

        /// <summary>
        /// 쿠폰 유효성 검증
        /// </summary>
        [HttpPost("coupons/{couponCode}/validate")]
        public async Task<ActionResult<DiscountValidationResult>> ValidateCoupon(string couponCode, [FromBody] BookingInfo bookingInfo)
        {
            try
            {
                var result = await _discountService.ValidateCouponAsync(couponCode, bookingInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "쿠폰 유효성 검증 중 오류 발생");
                return StatusCode(500, new { message = "유효성 검증 중 오류가 발생했습니다." });
            }
        }
    }

    // DTO 클래스들
    public class DiscountCalculationRequest
    {
        public BookingInfo BookingInfo { get; set; }
        public List<int> DiscountConditionIds { get; set; }
        public List<string> CouponCodes { get; set; }
    }

    public class CouponUsageRequest
    {
        public string UserId { get; set; }
        public string CouponCode { get; set; }
        public BookingInfo BookingInfo { get; set; }
        public string OrderNumber { get; set; }
    }
}