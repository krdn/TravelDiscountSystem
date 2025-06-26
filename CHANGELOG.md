# 변경 로그

이 프로젝트의 모든 주요 변경사항이 기록됩니다.

형식은 [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)를 기반으로 하며, [Semantic Versioning](https://semver.org/spec/v2.0.0.html)을 따릅니다.

## [1.0.0] - 2025-06-26

### 추가됨
- 여행상품 할인/쿠폰 관리 시스템 초기 버전
- 프로모션 관리 기능
- 할인조건 관리 기능 (즉시할인/기간할인)
- 할인쿠폰 관리 기능
- 복합 할인 계산 엔진
- 지역별/항공사별/인원별 세부 조건 처리
- RESTful API 제공
- Entity Framework Core 기반 데이터 액세스
- Swagger API 문서화
- 단위 테스트 (xUnit)
- 엑셀 테스트 케이스 기반 검증

### 핵심 기능
- **할인 계산 API**: 복잡한 할인 로직을 자동으로 계산
- **조건부 할인**: 지역, 항공사, 날짜, 인원 등 다양한 조건 지원
- **소수점 처리**: 정률 할인 시 소수점 올림 처리
- **중복 할인**: 할인조건과 할인쿠폰 동시 적용
- **유효성 검증**: 모든 할인 조건에 대한 엄격한 검증

### 기술 스택
- ASP.NET Core 6.0
- Entity Framework Core 6.0
- SQL Server
- xUnit 테스트 프레임워크
- Swagger/OpenAPI

### 테스트 케이스
- 즉시할인 (정액): 성인 5만원 할인
- 즉시할인 (정률): 3% 할인 소수점 올림
- 할인쿠폰: 항공사별 조건부 할인
- 지역 조건: 일본 5% 할인 (오키나와 제외)
- 복합 할인: 할인조건 + 쿠폰 중복 적용
- 기간 할인: 출발일 20일 전 조건
- 유효성 검증: 최소금액, 기간조건 등

### 보안
- 입력값 유효성 검증
- SQL 인젝션 방지 (EF Core 사용)
- 감사 로그 기능

### 성능
- 데이터베이스 인덱스 최적화
- 쿼리 성능 최적화
- 메모리 효율적인 할인 계산

### 문서화
- README.md: 프로젝트 개요 및 설치 가이드
- API_GUIDE.md: API 사용법 상세 설명
- DEPLOYMENT.md: 배포 가이드
- 코드 주석 및 XML 문서

### 설치 및 실행
```bash
git clone https://github.com/krdn/TravelDiscountSystem.git
cd TravelDiscountSystem
dotnet restore
dotnet ef database update --project src/TravelDiscountSystem
dotnet run --project src/TravelDiscountSystem
```

### API 엔드포인트
- `POST /api/discount/calculate`: 할인 금액 계산
- `POST /api/discount/conditions/applicable`: 적용 가능한 할인조건 조회
- `POST /api/discount/coupons/applicable`: 적용 가능한 쿠폰 조회
- `GET /api/promotion`: 프로모션 관리
- `GET /api/discountcondition`: 할인조건 관리
- `GET /api/discountcoupon`: 할인쿠폰 관리

### 알려진 제한사항
- 현재 버전에서는 기본 인증만 지원 (JWT 토큰은 향후 추가 예정)
- 실시간 재고 연동 기능 없음 (향후 추가 예정)
- 다국어 지원 없음 (한국어만 지원)

### 향후 계획
- JWT 기반 인증/인가 시스템
- 실시간 재고 연동
- 마이크로서비스 아키텍처 분리
- 다국어 지원
- 모바일 앱 API 확장
- Redis 캐싱 시스템
- 분산 로깅 시스템

---

## 버전 관리 규칙

- **Major (X.0.0)**: 호환성이 깨지는 변경
- **Minor (0.X.0)**: 새로운 기능 추가 (하위 호환)
- **Patch (0.0.X)**: 버그 수정 및 성능 개선

## 기여 가이드

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 감사의 말

- 엑셀 테스트 케이스 제공자
- ASP.NET Core 팀
- Entity Framework Core 팀
- 오픈소스 커뮤니티