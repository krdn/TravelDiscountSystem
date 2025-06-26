# 여행상품 할인/쿠폰 관리 시스템

## 🎯 프로젝트 개요

업로드된 엑셀 파일을 기반으로 분석한 **여행상품 할인/쿠폰 관리 시스템**입니다. 
복잡한 할인 조건과 쿠폰 로직을 체계적으로 관리하고, 정확한 할인 금액을 계산하는 C# 기반 웹 API 시스템입니다.

## 📊 시스템 분석 결과

### 핵심 도메인
- **프로모션(Promotion)**: 전체적인 마케팅 캠페인 관리
- **할인조건(DiscountCondition)**: 즉시할인/기간할인 등의 조건부 할인
- **할인쿠폰(DiscountCoupon)**: 코드 기반 할인쿠폰

### 복잡한 비즈니스 로직
- ✅ 다중 할인 조건 조합 (할인조건 + 할인쿠폰 중복 적용)
- ✅ 지역/항공사/상품별 세부 조건 필터링
- ✅ 인원별 차등 할인 (성인/소아N/소아E/유아/랜드)
- ✅ 기간 기반 할인 (출발일 기준 n일 전)
- ✅ 정액/정률 할인 및 소수점 처리

## 🏗️ 아키텍처

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web API       │    │   Business      │    │   Data Access   │
│   Controllers   │───▶│   Services      │───▶│   Repositories  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                       ┌─────────────────┐
                       │   Domain        │
                       │   Models        │
                       └─────────────────┘
```

### 주요 구성 요소

1. **Domain Layer**: 엔터티, 값 객체, 도메인 서비스
2. **Service Layer**: 비즈니스 로직 및 할인 계산
3. **Data Access Layer**: Entity Framework Core 기반 Repository
4. **Web API Layer**: RESTful API 컨트롤러

## 🚀 기술 스택

- **Backend**: ASP.NET Core 6.0, C# 10
- **Database**: MS SQL Server, Entity Framework Core
- **Testing**: xUnit, In-Memory Database
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture, Repository Pattern

## 📋 주요 기능

### 1. 할인 계산 API
```http
POST /api/discount/calculate
```
- 예약 정보에 따른 할인 금액 자동 계산
- 복합 할인 조건 적용 (할인조건 + 쿠폰 중복)
- 상세한 계산 과정 제공

### 2. 할인 조건 관리
```http
GET /api/discountcondition
POST /api/discountcondition
PUT /api/discountcondition/{id}
DELETE /api/discountcondition/{id}
```

### 3. 할인 쿠폰 관리
```http
GET /api/discountcoupon
POST /api/discountcoupon
PUT /api/discountcoupon/{id}
DELETE /api/discountcoupon/{id}
```

### 4. 프로모션 관리
```http
GET /api/promotion
POST /api/promotion
PUT /api/promotion/{id}
DELETE /api/promotion/{id}
```

## 🧪 테스트 케이스

엑셀 파일의 실제 테스트 케이스들을 단위 테스트로 구현:

### 케이스 1: 즉시할인 - 성인 5만원
```csharp
// 상품코드: 94215125, 성인가격: 1,015,100원
// 할인조건: 성인 50,000원 정액할인
// 예상결과: 할인 50,000원, 최종가격 965,100원
```

### 케이스 2: 즉시할인 - 3% 할인 (소수점 올림)
```csharp
// 복합 상품 (성인+소아E+랜드)
// 할인조건: 3% 정률할인, 소수점 올림 처리
```

### 케이스 6: 할인쿠폰 - 항공사 조건
```csharp
// 대한항공 조건부 5,000원 할인쿠폰
// 항공사 매칭 시에만 할인 적용
```

### 케이스 7-8: 지역 조건부 쿠폰
```csharp
// 일본 5% 할인쿠폰 (오키나와 제외)
// 오키나와: 할인 미적용
// 오사카: 할인 적용 (최대 20,000원)
```

### 케이스 9: 복합 할인
```csharp
// 할인조건(50,000원) + 할인쿠폰(5% 최대 20,000원)
// 총 할인: 70,000원
```

### 케이스 10: 기간 할인
```csharp
// 출발일 20일 전까지 예약 시 30,000원 할인
// 기간 조건 검증 로직 포함
```

## 🛠️ 설치 및 실행

### 1. 사전 요구사항
- .NET 6.0 SDK
- SQL Server (LocalDB 가능)
- Visual Studio 2022 또는 VS Code

### 2. 프로젝트 설정
```bash
# 프로젝트 복제
git clone https://github.com/krdn/TravelDiscountSystem.git
cd TravelDiscountSystem

# NuGet 패키지 복원
dotnet restore

# 데이터베이스 마이그레이션
dotnet ef database update

# 애플리케이션 실행
dotnet run
```

### 3. 데이터베이스 연결 설정
`appsettings.json` 파일의 연결 문자열 수정:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelDiscountSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. API 테스트
브라우저에서 `https://localhost:5001/swagger` 접속하여 Swagger UI 확인

## 📊 데이터베이스 스키마

### 주요 테이블
- `Promotions`: 프로모션 정보
- `DiscountConditions`: 할인조건 정보  
- `DiscountCoupons`: 할인쿠폰 정보
- `PromotionDiscountConditions`: 프로모션-할인조건 연결
- `PromotionDiscountCoupons`: 프로모션-할인쿠폰 연결
- `CouponUsageHistories`: 쿠폰 사용 이력

## 📞 문의 및 지원

개발 관련 문의사항이나 개선 제안이 있으시면 언제든 연락주세요.

---

**여행상품 할인/쿠폰 관리 시스템** - 복잡한 할인 로직을 단순하고 정확하게! 🎯