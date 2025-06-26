using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelDiscountSystem.Data;
using TravelDiscountSystem.Domain;
using TravelDiscountSystem.Services;
using Xunit;

namespace TravelDiscountSystem.Tests
{
    public class DiscountTestCases : IClassFixture<TestFixture>
    {
        private readonly IDiscountService _discountService;
        private readonly TestFixture _fixture;

        public DiscountTestCases(TestFixture fixture)
        {
            _fixture = fixture;
            _discountService = fixture.ServiceProvider.GetService<IDiscountService>();
        }

        [Fact]
        public async Task TestCase1_즉시할인_성인5만원할인()
        {
            // Arrange - 케이스1: 할인조건(즉시할인)-성인 5만원
            var bookingInfo = new BookingInfo
            {
                ProductCode = "94215125",
                AdultPrice = 1015100,
                AdultCount = 1,
                ChildNCount = 0,
                ChildECount = 0,
                InfantCount = 0,
                LandCount = 0,
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser1"
            };

            var discountConditionIds = new List<int> { 3 }; // 할인조건번호 3

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, discountConditionIds);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1015100, result.OriginalAmount);
            Assert.Equal(50000, result.TotalDiscountAmount);
            Assert.Equal(965100, result.FinalAmount);
            Assert.Single(result.AppliedDiscounts);
            Assert.Equal("DiscountCondition", result.AppliedDiscounts[0].Type);
        }

        [Fact]
        public async Task TestCase2_즉시할인_3퍼센트할인_소수점올림()
        {
            // Arrange - 케이스2: 할인조건(즉시할인)-3% 할인(소수점올림)
            var bookingInfo = new BookingInfo
            {
                ProductCode = "94215142",
                AdultPrice = 1065100,
                AdultCount = 1,
                ChildEPrice = 1065100,
                ChildECount = 1,
                LandPrice = 958590,
                LandCount = 1,
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser2"
            };

            var discountConditionIds = new List<int> { 26 }; // 할인조건번호 26

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, discountConditionIds);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(3088790, result.OriginalAmount); // 1065100 + 1065100 + 958590
            Assert.Equal(Math.Ceiling(3088790 * 0.03m), result.TotalDiscountAmount); // 3% 소수점 올림
        }

        [Fact]
        public async Task TestCase6_할인쿠폰_항공사조건_대한항공5천원할인()
        {
            // Arrange - 케이스6: 할인쿠폰-사용조건 항공사-대한항공 5천원 할인
            var bookingInfo = new BookingInfo
            {
                ProductCode = "93944599",
                AdultPrice = 1039000,
                AdultCount = 1,
                Airline = "대한항공",
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser6"
            };

            var couponCodes = new List<string> { "CMP-SOMU-RJMQ-TGKY" };

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, null, couponCodes);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1039000, result.OriginalAmount);
            Assert.Equal(5000, result.TotalDiscountAmount);
            Assert.Equal(1034000, result.FinalAmount);
            Assert.Single(result.AppliedDiscounts);
            Assert.Equal("DiscountCoupon", result.AppliedDiscounts[0].Type);
        }

        [Fact]
        public async Task TestCase7_할인쿠폰_지역조건_오키나와제외_할인미적용()
        {
            // Arrange - 케이스7: 할인쿠폰-사용조건 [지역]적용국가-일본, 제외도시-오키나와 일때 오키나와상품에 할인 미적용
            var bookingInfo = new BookingInfo
            {
                ProductCode = "93901299",
                AdultPrice = 709000,
                AdultCount = 1,
                DestinationCountry = "일본",
                DestinationCity = "오키나와",
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser7"
            };

            var couponCodes = new List<string> { "CMP-VJCK-895K-N7I2" };

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, null, couponCodes);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(709000, result.OriginalAmount);
            Assert.Equal(0, result.TotalDiscountAmount); // 제외도시이므로 할인 미적용
            Assert.Equal(709000, result.FinalAmount);
            Assert.Empty(result.AppliedDiscounts);
            Assert.Contains("제외 대상 도시입니다", result.WarningMessages);
        }

        [Fact]
        public async Task TestCase8_할인쿠폰_지역조건_오사카_할인적용()
        {
            // Arrange - 케이스8: 할인쿠폰-사용조건 [지역]적용국가-일본, 제외도시-오키나와 일때 오사카상품에 할인 적용 확인
            var bookingInfo = new BookingInfo
            {
                ProductCode = "93341552",
                AdultPrice = 907900,
                AdultCount = 1,
                DestinationCountry = "일본",
                DestinationCity = "오사카",
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser8"
            };

            var couponCodes = new List<string> { "CMP-VJCK-895K-N7I2" };

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, null, couponCodes);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(907900, result.OriginalAmount);
            Assert.Equal(20000, result.TotalDiscountAmount); // 5% 할인, 최대 20000원
            Assert.Equal(887900, result.FinalAmount);
            Assert.Single(result.AppliedDiscounts);
        }

        [Fact]
        public async Task TestCase9_복합할인_할인조건과쿠폰중복적용()
        {
            // Arrange - 케이스9: 할인조건(즉시할인)+할인쿠폰
            var bookingInfo = new BookingInfo
            {
                ProductCode = "94441844",
                AdultPrice = 1364200,
                AdultCount = 1,
                DestinationCountry = "일본",
                DestinationCity = "도쿄",
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser9"
            };

            var discountConditionIds = new List<int> { 3 }; // 할인조건번호 3 (5만원 할인)
            var couponCodes = new List<string> { "CMP-VJCK-895K-N7I2" }; // 5% 할인쿠폰

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, discountConditionIds, couponCodes);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(1364200, result.OriginalAmount);
            Assert.Equal(70000, result.TotalDiscountAmount); // 50000 + 20000 (최대한도)
            Assert.Equal(1294200, result.FinalAmount);
            Assert.Equal(2, result.AppliedDiscounts.Count);
        }

        [Fact]
        public async Task TestCase10_기간할인_20일전조건()
        {
            // Arrange - 케이스10: 할인조건(기간할인)-20일 전부터 3만원 할인
            var bookingInfo = new BookingInfo
            {
                ProductCode = "91547501",
                AdultPrice = 2842800,
                AdultCount = 1,
                ChildEPrice = 2842800,
                ChildECount = 1,
                DepartureDate = DateTime.Now.AddDays(25), // 25일 후 출발 (20일 조건 충족)
                BookingDate = DateTime.Now,
                UserId = "testuser10"
            };

            var discountConditionIds = new List<int> { 22 }; // 할인조건번호 22

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, discountConditionIds);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(5685600, result.OriginalAmount); // 2842800 * 2
            Assert.Equal(60000, result.TotalDiscountAmount); // 성인 + 소아E 각각 30000원
            Assert.Equal(5625600, result.FinalAmount);
        }

        [Fact]
        public async Task TestCase_기간할인_조건미충족()
        {
            // Arrange - 기간할인 조건 미충족 테스트
            var bookingInfo = new BookingInfo
            {
                ProductCode = "91547501",
                AdultPrice = 2842800,
                AdultCount = 1,
                DepartureDate = DateTime.Now.AddDays(15), // 15일 후 출발 (20일 조건 미충족)
                BookingDate = DateTime.Now,
                UserId = "testuser_period_fail"
            };

            var discountConditionIds = new List<int> { 22 }; // 할인조건번호 22 (20일 전 조건)

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, discountConditionIds);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2842800, result.OriginalAmount);
            Assert.Equal(0, result.TotalDiscountAmount); // 조건 미충족으로 할인 미적용
            Assert.Equal(2842800, result.FinalAmount);
            Assert.Empty(result.AppliedDiscounts);
            Assert.Contains("출발일 20일 전까지만 예약 가능합니다", result.WarningMessages);
        }

        [Fact]
        public async Task TestCase_최소금액조건_미충족()
        {
            // Arrange - 최소 적용 금액 미충족 테스트
            var bookingInfo = new BookingInfo
            {
                ProductCode = "test001",
                AdultPrice = 100000, // 최소 적용 금액보다 낮음
                AdultCount = 1,
                DepartureDate = DateTime.Now.AddDays(30),
                BookingDate = DateTime.Now,
                UserId = "testuser_min_amount_fail"
            };

            // 최소 적용 금액이 200000원인 쿠폰 사용
            var couponCodes = new List<string> { "CMP-VJCK-895K-N7I2" };

            // Act
            var result = await _discountService.CalculateDiscountAsync(bookingInfo, null, couponCodes);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(100000, result.OriginalAmount);
            Assert.Equal(0, result.TotalDiscountAmount); // 최소 금액 미충족으로 할인 미적용
            Assert.Equal(100000, result.FinalAmount);
            Assert.Empty(result.AppliedDiscounts);
            Assert.Contains("최소 적용 금액", result.WarningMessages[0]);
        }
    }

    // 테스트 픽스처
    public class TestFixture : IDisposable
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public TestFixture()
        {
            var services = new ServiceCollection();
            
            // In-Memory 데이터베이스 설정
            services.AddDbContext<TravelDiscountContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Repository 등록
            services.AddScoped<IDiscountConditionRepository, DiscountConditionRepository>();
            services.AddScoped<IDiscountCouponRepository, DiscountCouponRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<ICouponUsageHistoryRepository, CouponUsageHistoryRepository>();

            // Service 등록
            services.AddScoped<IDiscountService, DiscountService>();

            // 로깅 설정
            services.AddLogging();

            ServiceProvider = services.BuildServiceProvider();

            // 테스트 데이터 초기화
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TravelDiscountContext>();
            context.Database.EnsureCreated();

            // 테스트용 할인조건 데이터
            var discountConditions = new List<DiscountCondition>
            {
                new DiscountCondition
                {
                    Id = 3,
                    ConditionNumber = 3,
                    DiscountType = "즉시할인",
                    Description = "정액할인-성인 5만원",
                    IsEnabled = true,
                    AmountType = "정액",
                    DiscountValue = 50000,
                    ApplicableTarget = "어른"
                },
                new DiscountCondition
                {
                    Id = 26,
                    ConditionNumber = 26,
                    DiscountType = "즉시할인",
                    Description = "정률할인-3% 할인",
                    IsEnabled = true,
                    AmountType = "정률",
                    DiscountValue = 0.03m,
                    ApplicableTarget = "전체",
                    MinimumAmount = 0,
                    MaximumDiscountAmount = 100000
                },
                new DiscountCondition
                {
                    Id = 22,
                    ConditionNumber = 22,
                    DiscountType = "기간할인",
                    Description = "기간할인-20일 전부터 3만원",
                    IsEnabled = true,
                    AmountType = "정액",
                    DiscountValue = 30000,
                    ApplicableTarget = "전체",
                    DaysBeforeDeparture = 20
                }
            };

            // 테스트용 할인쿠폰 데이터
            var discountCoupons = new List<DiscountCoupon>
            {
                new DiscountCoupon
                {
                    Id = 1,
                    CouponCode = "CMP-SOMU-RJMQ-TGKY",
                    Description = "항공사-대한항공 5천원 할인",
                    IssueStatus = "발행중",
                    CouponName = "대한항공 할인쿠폰",
                    IssueStartDate = DateTime.Now.AddDays(-30),
                    IssueEndDate = DateTime.Now.AddDays(30),
                    IsFixedDiscount = true,
                    DiscountAmount = 5000,
                    HasAirlineCondition = true,
                    AirlineConditions = "대한항공"
                },
                new DiscountCoupon
                {
                    Id = 2,
                    CouponCode = "CMP-VJCK-895K-N7I2",
                    Description = "일본 5% 할인 (오키나와 제외)",
                    IssueStatus = "발행중",
                    CouponName = "일본여행 할인쿠폰",
                    IssueStartDate = DateTime.Now.AddDays(-30),
                    IssueEndDate = DateTime.Now.AddDays(30),
                    IsPercentageDiscount = true,
                    DiscountRate = 0.05m,
                    MinimumAmountForRate = 200000,
                    MaximumDiscountForRate = 20000,
                    HasCountryCondition = true,
                    ApplicableCountries = "일본",
                    HasCityCondition = true,
                    ExcludedCities = "오키나와"
                }
            };

            context.DiscountConditions.AddRange(discountConditions);
            context.DiscountCoupons.AddRange(discountCoupons);
            context.SaveChanges();
        }

        public void Dispose()
        {
            ServiceProvider?.Dispose();
        }
    }
}