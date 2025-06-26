using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelDiscountSystem.Domain
{
    // 기본 엔터티 베이스 클래스
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    // 프로모션 엔터티
    public class Promotion : BaseEntity
    {
        public int PromotionNumber { get; set; }
        public string Type { get; set; } // 전시프로모션, 접두어 등
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } // 진행중, 종료, 대기
        public bool IsProposalLinked { get; set; }
        public string ProposalNumber { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal SupportAmount { get; set; }
        
        // 접두어 정보
        public bool HasPrefix { get; set; }
        public string PrefixCode { get; set; }
        public bool IsEnabled { get; set; }
        public bool HasIcon { get; set; }
        
        // 적용 조건
        public string ProductCategory { get; set; } // 전체, 패키지, 항공 등
        public string DepartureLocation { get; set; }
        public bool IsDepartureConfirmed { get; set; }
        public DateTime? ApplicationStartDate { get; set; }
        public DateTime? ApplicationEndDate { get; set; }
        public string ApplicableCountries { get; set; }
        public string ExcludedCountries { get; set; }
        public string ApplicableCities { get; set; }
        public string ExcludedCities { get; set; }
        public string Airlines { get; set; }
        
        // 노출 설정
        public bool IsB2CVisible { get; set; }
        public bool IsB2CRateApplied { get; set; }
        public bool IsBPVisible { get; set; }
        public bool IsMobileVisible { get; set; }
        public bool IsAppVisible { get; set; }
        
        // 연관 관계
        public virtual ICollection<PromotionDiscountCondition> DiscountConditions { get; set; } = new List<PromotionDiscountCondition>();
        public virtual ICollection<PromotionDiscountCoupon> DiscountCoupons { get; set; } = new List<PromotionDiscountCoupon>();
    }

    // 할인조건 엔터티
    public class DiscountCondition : BaseEntity
    {
        public int ConditionNumber { get; set; }
        public string DiscountType { get; set; } // 즉시할인, 기간할인
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string AmountType { get; set; } // 정액, 정률
        public decimal DiscountValue { get; set; } // 할인금액 또는 할인율
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public string ApplicableTarget { get; set; } // 전체, 어른, 소아N, 소아E, 유아, 랜드
        
        // 기간할인용 필드
        public int? DaysBeforeDeparture { get; set; }
        
        // 인원별 할인 조건
        public bool IsWholeDiscount { get; set; }
        public int? AdultBaseCount { get; set; }
        public int? AdultDiscountCount { get; set; }
        public string GenderCondition { get; set; }
        public int? ChildNBaseCount { get; set; }
        public int? ChildNDiscountCount { get; set; }
        public int? ChildEBaseCount { get; set; }
        public int? ChildEDiscountCount { get; set; }
        public int? InfantBaseCount { get; set; }
        public int? InfantDiscountCount { get; set; }
        public int? LandBaseCount { get; set; }
        public int? LandDiscountCount { get; set; }
        public bool IsOneOfNDiscount { get; set; }
        public string GroupNumber { get; set; }
        
        // 예상 할인금액 (성인 1인 기준)
        public decimal EstimatedDiscountAmount { get; set; }
        public decimal EstimatedSellingPrice { get; set; }
        
        // 연관 관계
        public virtual ICollection<PromotionDiscountCondition> PromotionConditions { get; set; } = new List<PromotionDiscountCondition>();
    }

    // 할인쿠폰 엔터티
    public class DiscountCoupon : BaseEntity
    {
        public string CouponCode { get; set; }
        public string Description { get; set; }
        public string IssueStatus { get; set; } // 발행중, 종료, 대기
        public string IssueType { get; set; } // 일반할인쿠폰, 특별할인쿠폰
        public string CouponName { get; set; }
        public string TargetProducts { get; set; } // 패키지, 항공, 호텔 등
        public string PackageType { get; set; }
        
        // 발행 정보
        public DateTime IssueStartDate { get; set; }
        public DateTime IssueEndDate { get; set; }
        public string PaymentMethod { get; set; } // ALL, 카드, 무통장입금 등
        public string AppType { get; set; } // ALL, 모바일, PC
        public bool IsProductBased { get; set; }
        public bool IsCouponNameRequired { get; set; }
        public int IssueQuantity { get; set; }
        
        // 유효기간
        public bool IsValidByDays { get; set; }
        public bool IsValidByMonths { get; set; }
        public int ValidityPeriod { get; set; }
        public bool IsProductVisible { get; set; }
        
        // 할인 정보
        public bool IsPercentageDiscount { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? MinimumAmountForRate { get; set; }
        public decimal? MaximumDiscountForRate { get; set; }
        public bool IsFixedDiscount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? MinimumAmountForFixed { get; set; }
        
        // 사용 조건 - 지역
        public bool HasCountryCondition { get; set; }
        public string ApplicableCountries { get; set; }
        public string ExcludedCountries { get; set; }
        public bool HasCityCondition { get; set; }
        public string ApplicableCities { get; set; }
        public string ExcludedCities { get; set; }
        
        // 사용 조건 - 기타
        public bool HasProductCondition { get; set; }
        public string ProductConditions { get; set; }
        public bool HasHotelCondition { get; set; }
        public string HotelConditions { get; set; }
        public bool HasAirlineCondition { get; set; }
        public string AirlineConditions { get; set; }
        public bool HasLandCondition { get; set; }
        public string LandConditions { get; set; }
        public bool HasDateCondition { get; set; }
        public DateTime? AvailableStartDate { get; set; }
        public DateTime? AvailableEndDate { get; set; }
        
        // 예산 정보
        public decimal BudgetSupportAmount { get; set; }
        public decimal PromotionIssueAmount { get; set; }
        
        // 적용 대상 상품코드
        public string ApplicableProductCodes { get; set; }
        public string Remarks { get; set; }
        
        // 연관 관계
        public virtual ICollection<PromotionDiscountCoupon> PromotionCoupons { get; set; } = new List<PromotionDiscountCoupon>();
        public virtual ICollection<CouponUsageHistory> UsageHistories { get; set; } = new List<CouponUsageHistory>();
    }

    // 프로모션-할인조건 연결 테이블
    public class PromotionDiscountCondition : BaseEntity
    {
        public int PromotionId { get; set; }
        public int DiscountConditionId { get; set; }
        public int Priority { get; set; } // 적용 우선순위
        public bool IsActive { get; set; } = true;
        
        public virtual Promotion Promotion { get; set; }
        public virtual DiscountCondition DiscountCondition { get; set; }
    }

    // 프로모션-할인쿠폰 연결 테이블
    public class PromotionDiscountCoupon : BaseEntity
    {
        public int PromotionId { get; set; }
        public int DiscountCouponId { get; set; }
        public int Priority { get; set; } // 적용 우선순위
        public bool IsActive { get; set; } = true;
        
        public virtual Promotion Promotion { get; set; }
        public virtual DiscountCoupon DiscountCoupon { get; set; }
    }

    // 쿠폰 사용 이력
    public class CouponUsageHistory : BaseEntity
    {
        public int DiscountCouponId { get; set; }
        public string UserId { get; set; }
        public string ProductCode { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime UsedAt { get; set; }
        public string OrderNumber { get; set; }
        public string Status { get; set; } // 사용완료, 취소
        
        public virtual DiscountCoupon DiscountCoupon { get; set; }
    }

    // 예약 정보 (테스트 케이스용)
    public class BookingInfo
    {
        public string ProductCode { get; set; }
        public decimal AdultPrice { get; set; }
        public decimal? ChildNPrice { get; set; }
        public decimal? ChildEPrice { get; set; }
        public decimal? InfantPrice { get; set; }
        public decimal? LandPrice { get; set; }
        public int AdultCount { get; set; }
        public int ChildNCount { get; set; }
        public int ChildECount { get; set; }
        public int InfantCount { get; set; }
        public int LandCount { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime BookingDate { get; set; }
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }
        public string Airline { get; set; }
        public string UserId { get; set; }
    }

    // 할인 계산 결과
    public class DiscountCalculationResult
    {
        public decimal OriginalAmount { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<AppliedDiscount> AppliedDiscounts { get; set; } = new List<AppliedDiscount>();
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public List<string> WarningMessages { get; set; } = new List<string>();
    }

    // 적용된 할인 정보
    public class AppliedDiscount
    {
        public string Type { get; set; } // DiscountCondition, DiscountCoupon
        public string Code { get; set; } // 할인조건번호 또는 쿠폰코드
        public string Name { get; set; }
        public decimal DiscountAmount { get; set; }
        public string CalculationDetail { get; set; } // 계산 과정 설명
        public int Priority { get; set; }
    }

    // 할인 유효성 검증 결과
    public class DiscountValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public List<string> WarningMessages { get; set; } = new List<string>();
    }

    // 열거형 정의
    public enum DiscountType
    {
        Immediate,  // 즉시할인
        Period      // 기간할인
    }

    public enum AmountType
    {
        Fixed,      // 정액
        Percentage  // 정률
    }

    public enum ApplicableTarget
    {
        All,        // 전체
        Adult,      // 어른
        ChildN,     // 소아N
        ChildE,     // 소아E
        Infant,     // 유아
        Land        // 랜드
    }

    public enum PromotionStatus
    {
        Active,     // 진행중
        Ended,      // 종료
        Waiting     // 대기
    }

    public enum CouponIssueStatus
    {
        Issuing,    // 발행중
        Ended,      // 종료
        Waiting     // 대기
    }
}