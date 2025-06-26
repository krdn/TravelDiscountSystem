using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TravelDiscountSystem.Domain;
using TravelDiscountSystem.Data;

namespace TravelDiscountSystem.Services
{
    public interface IDiscountService
    {
        Task<DiscountCalculationResult> CalculateDiscountAsync(BookingInfo bookingInfo, 
            List<int> discountConditionIds = null, 
            List<string> couponCodes = null);
            
        Task<DiscountValidationResult> ValidateDiscountConditionAsync(int conditionId, BookingInfo bookingInfo);
        Task<DiscountValidationResult> ValidateCouponAsync(string couponCode, BookingInfo bookingInfo);
        Task<List<DiscountCondition>> GetApplicableDiscountConditionsAsync(BookingInfo bookingInfo);
        Task<List<DiscountCoupon>> GetApplicableCouponsAsync(BookingInfo bookingInfo);
    }

    public class DiscountService : IDiscountService
    {
        private readonly IDiscountConditionRepository _discountConditionRepository;
        private readonly IDiscountCouponRepository _discountCouponRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(
            IDiscountConditionRepository discountConditionRepository,
            IDiscountCouponRepository discountCouponRepository,
            IPromotionRepository promotionRepository,
            ILogger<DiscountService> logger)
        {
            _discountConditionRepository = discountConditionRepository;
            _discountCouponRepository = discountCouponRepository;
            _promotionRepository = promotionRepository;
            _logger = logger;
        }

        public async Task<DiscountCalculationResult> CalculateDiscountAsync(
            BookingInfo bookingInfo, 
            List<int> discountConditionIds = null, 
            List<string> couponCodes = null)
        {
            var result = new DiscountCalculationResult
            {
                OriginalAmount = CalculateOriginalAmount(bookingInfo),
                IsSuccess = true
            };

            try
            {
                _logger.LogInformation($"할인 계산 시작 - 상품코드: {bookingInfo.ProductCode}");

                // 1. 적용 가능한 할인조건 조회
                var applicableConditions = discountConditionIds != null 
                    ? await GetDiscountConditionsByIdsAsync(discountConditionIds)
                    : await GetApplicableDiscountConditionsAsync(bookingInfo);

                // 2. 적용 가능한 쿠폰 조회
                var applicableCoupons = couponCodes != null
                    ? await GetCouponsByCodesAsync(couponCodes)
                    : await GetApplicableCouponsAsync(bookingInfo);

                // 3. 할인조건 적용 (우선순위 순)
                foreach (var condition in applicableConditions.OrderBy(c => c.Id))
                {
                    var validation = await ValidateDiscountConditionAsync(condition.Id, bookingInfo);
                    if (validation.IsValid)
                    {
                        var discountAmount = CalculateConditionDiscount(condition, bookingInfo, result.OriginalAmount);
                        if (discountAmount > 0)
                        {
                            result.AppliedDiscounts.Add(new AppliedDiscount
                            {
                                Type = "DiscountCondition",
                                Code = condition.ConditionNumber.ToString(),
                                Name = condition.Description,
                                DiscountAmount = discountAmount,
                                CalculationDetail = GetConditionCalculationDetail(condition, bookingInfo, discountAmount),
                                Priority = 1
                            });
                            result.TotalDiscountAmount += discountAmount;
                        }
                    }
                    else
                    {
                        result.WarningMessages.AddRange(validation.ErrorMessages);
                    }
                }

                // 4. 쿠폰 적용 (할인조건 적용 후)
                foreach (var coupon in applicableCoupons.OrderBy(c => c.Id))
                {
                    var validation = await ValidateCouponAsync(coupon.CouponCode, bookingInfo);
                    if (validation.IsValid)
                    {
                        var currentAmount = result.OriginalAmount - result.TotalDiscountAmount;
                        var discountAmount = CalculateCouponDiscount(coupon, bookingInfo, currentAmount);
                        if (discountAmount > 0)
                        {
                            result.AppliedDiscounts.Add(new AppliedDiscount
                            {
                                Type = "DiscountCoupon",
                                Code = coupon.CouponCode,
                                Name = coupon.CouponName,
                                DiscountAmount = discountAmount,
                                CalculationDetail = GetCouponCalculationDetail(coupon, bookingInfo, discountAmount),
                                Priority = 2
                            });
                            result.TotalDiscountAmount += discountAmount;
                        }
                    }
                    else
                    {
                        result.WarningMessages.AddRange(validation.ErrorMessages);
                    }
                }

                // 5. 최종 금액 계산
                result.FinalAmount = Math.Max(0, result.OriginalAmount - result.TotalDiscountAmount);

                _logger.LogInformation($"할인 계산 완료 - 원금: {result.OriginalAmount:N0}, 할인: {result.TotalDiscountAmount:N0}, 최종: {result.FinalAmount:N0}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "할인 계산 중 오류 발생");
                result.IsSuccess = false;
                result.ErrorMessage = "할인 계산 중 오류가 발생했습니다.";
            }

            return result;
        }

        public async Task<DiscountValidationResult> ValidateDiscountConditionAsync(int conditionId, BookingInfo bookingInfo)
        {
            var result = new DiscountValidationResult { IsValid = true };
            var condition = await _discountConditionRepository.GetByIdAsync(conditionId);

            if (condition == null)
            {
                result.IsValid = false;
                result.ErrorMessages.Add("할인조건을 찾을 수 없습니다.");
                return result;
            }

            // 활성화 상태 확인
            if (!condition.IsEnabled)
            {
                result.IsValid = false;
                result.ErrorMessages.Add("비활성화된 할인조건입니다.");
                return result;
            }

            // 기간할인 조건 확인
            if (condition.DiscountType == "기간할인" && condition.DaysBeforeDeparture.HasValue)
            {
                var daysDifference = (bookingInfo.DepartureDate - bookingInfo.BookingDate).Days;
                if (daysDifference < condition.DaysBeforeDeparture.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add($"출발일 {condition.DaysBeforeDeparture}일 전까지만 예약 가능합니다.");
                    return result;
                }
            }

            // 최소 적용 금액 확인
            if (condition.MinimumAmount.HasValue)
            {
                var applicableAmount = GetApplicableAmountForCondition(condition, bookingInfo);
                if (applicableAmount < condition.MinimumAmount.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add($"최소 적용 금액({condition.MinimumAmount:N0}원)을 충족하지 않습니다.");
                    return result;
                }
            }

            // 적용 대상 확인
            if (!IsTargetApplicable(condition.ApplicableTarget, bookingInfo))
            {
                result.IsValid = false;
                result.ErrorMessages.Add("할인 적용 대상이 아닙니다.");
                return result;
            }

            return result;
        }

        public async Task<DiscountValidationResult> ValidateCouponAsync(string couponCode, BookingInfo bookingInfo)
        {
            var result = new DiscountValidationResult { IsValid = true };
            var coupon = await _discountCouponRepository.GetByCouponCodeAsync(couponCode);

            if (coupon == null)
            {
                result.IsValid = false;
                result.ErrorMessages.Add("쿠폰을 찾을 수 없습니다.");
                return result;
            }

            // 발행 상태 확인
            if (coupon.IssueStatus != "발행중")
            {
                result.IsValid = false;
                result.ErrorMessages.Add("사용할 수 없는 쿠폰입니다.");
                return result;
            }

            // 유효기간 확인
            var currentDate = DateTime.Now;
            if (currentDate < coupon.IssueStartDate || currentDate > coupon.IssueEndDate)
            {
                result.IsValid = false;
                result.ErrorMessages.Add("쿠폰 사용 기간이 아닙니다.");
                return result;
            }

            // 지역 조건 확인
            if (coupon.HasCountryCondition)
            {
                if (!string.IsNullOrEmpty(coupon.ApplicableCountries) && 
                    !coupon.ApplicableCountries.Contains(bookingInfo.DestinationCountry))
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add("적용 대상 국가가 아닙니다.");
                    return result;
                }

                if (!string.IsNullOrEmpty(coupon.ExcludedCountries) && 
                    coupon.ExcludedCountries.Contains(bookingInfo.DestinationCountry))
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add("제외 대상 국가입니다.");
                    return result;
                }
            }

            // 도시 조건 확인
            if (coupon.HasCityCondition)
            {
                if (!string.IsNullOrEmpty(coupon.ApplicableCities) && 
                    !coupon.ApplicableCities.Contains(bookingInfo.DestinationCity))
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add("적용 대상 도시가 아닙니다.");
                    return result;
                }

                if (!string.IsNullOrEmpty(coupon.ExcludedCities) && 
                    coupon.ExcludedCities.Contains(bookingInfo.DestinationCity))
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add("제외 대상 도시입니다.");
                    return result;
                }
            }

            // 항공사 조건 확인
            if (coupon.HasAirlineCondition && !string.IsNullOrEmpty(coupon.AirlineConditions))
            {
                if (!coupon.AirlineConditions.Contains(bookingInfo.Airline))
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add("적용 대상 항공사가 아닙니다.");
                    return result;
                }
            }

            // 최소 적용 금액 확인
            var totalAmount = CalculateOriginalAmount(bookingInfo);
            if (coupon.IsPercentageDiscount && coupon.MinimumAmountForRate.HasValue)
            {
                if (totalAmount < coupon.MinimumAmountForRate.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add($"최소 적용 금액({coupon.MinimumAmountForRate:N0}원)을 충족하지 않습니다.");
                    return result;
                }
            }

            if (coupon.IsFixedDiscount && coupon.MinimumAmountForFixed.HasValue)
            {
                if (totalAmount < coupon.MinimumAmountForFixed.Value)
                {
                    result.IsValid = false;
                    result.ErrorMessages.Add($"최소 적용 금액({coupon.MinimumAmountForFixed:N0}원)을 충족하지 않습니다.");
                    return result;
                }
            }

            return result;
        }

        public async Task<List<DiscountCondition>> GetApplicableDiscountConditionsAsync(BookingInfo bookingInfo)
        {
            var allConditions = await _discountConditionRepository.GetActiveConditionsAsync();
            var applicableConditions = new List<DiscountCondition>();

            foreach (var condition in allConditions)
            {
                var validation = await ValidateDiscountConditionAsync(condition.Id, bookingInfo);
                if (validation.IsValid)
                {
                    applicableConditions.Add(condition);
                }
            }

            return applicableConditions;
        }

        public async Task<List<DiscountCoupon>> GetApplicableCouponsAsync(BookingInfo bookingInfo)
        {
            var allCoupons = await _discountCouponRepository.GetActiveCouponsAsync();
            var applicableCoupons = new List<DiscountCoupon>();

            foreach (var coupon in allCoupons)
            {
                var validation = await ValidateCouponAsync(coupon.CouponCode, bookingInfo);
                if (validation.IsValid)
                {
                    applicableCoupons.Add(coupon);
                }
            }

            return applicableCoupons;
        }

        #region Private Methods

        private decimal CalculateOriginalAmount(BookingInfo bookingInfo)
        {
            return (bookingInfo.AdultPrice * bookingInfo.AdultCount) +
                   ((bookingInfo.ChildNPrice ?? 0) * bookingInfo.ChildNCount) +
                   ((bookingInfo.ChildEPrice ?? 0) * bookingInfo.ChildECount) +
                   ((bookingInfo.InfantPrice ?? 0) * bookingInfo.InfantCount) +
                   ((bookingInfo.LandPrice ?? 0) * bookingInfo.LandCount);
        }

        private decimal CalculateConditionDiscount(DiscountCondition condition, BookingInfo bookingInfo, decimal originalAmount)
        {
            decimal discountAmount = 0;
            var targetAmount = GetApplicableAmountForCondition(condition, bookingInfo);

            if (condition.AmountType == "정액")
            {
                discountAmount = condition.DiscountValue;
                
                // 인원별 할인 처리
                if (condition.ApplicableTarget == "어른" && bookingInfo.AdultCount > 0)
                    discountAmount *= bookingInfo.AdultCount;
                else if (condition.ApplicableTarget == "소아N" && bookingInfo.ChildNCount > 0)
                    discountAmount *= bookingInfo.ChildNCount;
                else if (condition.ApplicableTarget == "소아E" && bookingInfo.ChildECount > 0)
                    discountAmount *= bookingInfo.ChildECount;
                else if (condition.ApplicableTarget == "랜드" && bookingInfo.LandCount > 0)
                    discountAmount *= bookingInfo.LandCount;
            }
            else if (condition.AmountType == "정률")
            {
                discountAmount = targetAmount * condition.DiscountValue;
                discountAmount = Math.Ceiling(discountAmount); // 소수점 올림
            }

            // 최대 할인 금액 제한
            if (condition.MaximumDiscountAmount.HasValue)
                discountAmount = Math.Min(discountAmount, condition.MaximumDiscountAmount.Value);

            // 대상 금액을 초과할 수 없음
            discountAmount = Math.Min(discountAmount, targetAmount);

            return Math.Max(0, discountAmount);
        }

        private decimal CalculateCouponDiscount(DiscountCoupon coupon, BookingInfo bookingInfo, decimal currentAmount)
        {
            decimal discountAmount = 0;

            if (coupon.IsPercentageDiscount && coupon.DiscountRate.HasValue)
            {
                discountAmount = currentAmount * coupon.DiscountRate.Value;
                if (coupon.MaximumDiscountForRate.HasValue)
                    discountAmount = Math.Min(discountAmount, coupon.MaximumDiscountForRate.Value);
            }
            else if (coupon.IsFixedDiscount && coupon.DiscountAmount.HasValue)
            {
                discountAmount = coupon.DiscountAmount.Value;
            }

            discountAmount = Math.Min(discountAmount, currentAmount);
            return Math.Max(0, discountAmount);
        }

        private decimal GetApplicableAmountForCondition(DiscountCondition condition, BookingInfo bookingInfo)
        {
            switch (condition.ApplicableTarget)
            {
                case "어른":
                    return bookingInfo.AdultPrice * bookingInfo.AdultCount;
                case "소아N":
                    return (bookingInfo.ChildNPrice ?? 0) * bookingInfo.ChildNCount;
                case "소아E":
                    return (bookingInfo.ChildEPrice ?? 0) * bookingInfo.ChildECount;
                case "유아":
                    return (bookingInfo.InfantPrice ?? 0) * bookingInfo.InfantCount;
                case "랜드":
                    return (bookingInfo.LandPrice ?? 0) * bookingInfo.LandCount;
                case "전체":
                default:
                    return CalculateOriginalAmount(bookingInfo);
            }
        }

        private bool IsTargetApplicable(string applicableTarget, BookingInfo bookingInfo)
        {
            switch (applicableTarget)
            {
                case "어른": return bookingInfo.AdultCount > 0;
                case "소아N": return bookingInfo.ChildNCount > 0;
                case "소아E": return bookingInfo.ChildECount > 0;
                case "유아": return bookingInfo.InfantCount > 0;
                case "랜드": return bookingInfo.LandCount > 0;
                case "전체":
                default: return true;
            }
        }

        private string GetConditionCalculationDetail(DiscountCondition condition, BookingInfo bookingInfo, decimal discountAmount)
        {
            var target = condition.ApplicableTarget;
            var type = condition.AmountType;
            var value = condition.DiscountValue;

            if (type == "정액")
                return $"{target} {value:N0}원 할인 = {discountAmount:N0}원";
            else
            {
                var rate = (value * 100).ToString("0.##");
                return $"{target} {rate}% 할인 (소수점 올림) = {discountAmount:N0}원";
            }
        }

        private string GetCouponCalculationDetail(DiscountCoupon coupon, BookingInfo bookingInfo, decimal discountAmount)
        {
            if (coupon.IsPercentageDiscount && coupon.DiscountRate.HasValue)
            {
                var rate = (coupon.DiscountRate.Value * 100).ToString("0.##");
                return $"{rate}% 할인 (최대 {coupon.MaximumDiscountForRate:N0}원) = {discountAmount:N0}원";
            }
            else if (coupon.IsFixedDiscount && coupon.DiscountAmount.HasValue)
            {
                return $"{coupon.DiscountAmount:N0}원 할인 = {discountAmount:N0}원";
            }

            return $"할인 적용 = {discountAmount:N0}원";
        }

        private async Task<List<DiscountCondition>> GetDiscountConditionsByIdsAsync(List<int> ids)
        {
            var conditions = new List<DiscountCondition>();
            foreach (var id in ids)
            {
                var condition = await _discountConditionRepository.GetByIdAsync(id);
                if (condition != null) conditions.Add(condition);
            }
            return conditions;
        }

        private async Task<List<DiscountCoupon>> GetCouponsByCodesAsync(List<string> codes)
        {
            var coupons = new List<DiscountCoupon>();
            foreach (var code in codes)
            {
                var coupon = await _discountCouponRepository.GetByCouponCodeAsync(code);
                if (coupon != null) coupons.Add(coupon);
            }
            return coupons;
        }

        #endregion
    }
}