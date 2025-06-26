# 배포 가이드

## 시스템 요구사항

### 최소 요구사항
- OS: Windows Server 2019+ 또는 Ubuntu 20.04+
- RAM: 4GB 이상
- Storage: 20GB 이상
- .NET 6.0 Runtime
- SQL Server 2019+ 또는 PostgreSQL 13+

### 권장 요구사항
- OS: Windows Server 2022 또는 Ubuntu 22.04
- RAM: 8GB 이상
- Storage: 50GB 이상 (SSD 권장)
- .NET 6.0 Runtime
- SQL Server 2022 또는 PostgreSQL 15+
- Redis (캐싱용)

## 로컬 개발 환경 설정

### 1. 사전 요구사항 설치
```bash
# .NET 6.0 SDK 설치
winget install Microsoft.DotNet.SDK.6

# SQL Server LocalDB 설치 (Windows)
winget install Microsoft.SQLServerLocalDB

# 또는 Docker를 사용한 SQL Server 설치
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. 프로젝트 클론 및 설정
```bash
git clone https://github.com/krdn/TravelDiscountSystem.git
cd TravelDiscountSystem

# NuGet 패키지 복원
dotnet restore

# appsettings.json 설정
cp src/TravelDiscountSystem/appsettings.json src/TravelDiscountSystem/appsettings.Development.json
```

### 3. 데이터베이스 설정
```bash
# Entity Framework 도구 설치
dotnet tool install --global dotnet-ef

# 마이그레이션 생성
dotnet ef migrations add InitialCreate --project src/TravelDiscountSystem

# 데이터베이스 업데이트
dotnet ef database update --project src/TravelDiscountSystem
```

### 4. 애플리케이션 실행
```bash
cd src/TravelDiscountSystem
dotnet run
```

브라우저에서 `https://localhost:5001/swagger` 접속하여 API 문서 확인

## 운영 환경 배포

### Docker를 사용한 배포

#### 1. Dockerfile 생성
```dockerfile
# 파일: src/TravelDiscountSystem/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TravelDiscountSystem.csproj", "."]
RUN dotnet restore "TravelDiscountSystem.csproj"
COPY . .
RUN dotnet build "TravelDiscountSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TravelDiscountSystem.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TravelDiscountSystem.dll"]
```

#### 2. Docker Compose 설정
```yaml
# 파일: docker-compose.yml
version: '3.8'

services:
  webapp:
    build: ./src/TravelDiscountSystem
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TravelDiscountSystemDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - sqlserver
    networks:
      - app-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql
    networks:
      - app-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - app-network

volumes:
  sql-data:
  redis-data:

networks:
  app-network:
    driver: bridge
```

#### 3. 배포 실행
```bash
# Docker 이미지 빌드 및 실행
docker-compose up -d

# 로그 확인
docker-compose logs -f webapp

# 서비스 상태 확인
docker-compose ps
```

## 성능 최적화 및 모니터링

### 캐싱 전략
```csharp
// Redis 캐싱 설정
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// 캐싱 사용 예시
public async Task<List<DiscountCondition>> GetActiveConditionsAsync()
{
    var cacheKey = "active_conditions";
    var cached = await _cache.GetStringAsync(cacheKey);
    
    if (cached != null)
    {
        return JsonSerializer.Deserialize<List<DiscountCondition>>(cached);
    }
    
    var conditions = await _repository.GetActiveConditionsAsync();
    var serialized = JsonSerializer.Serialize(conditions);
    await _cache.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(30));
    
    return conditions;
}
```

### 애플리케이션 로깅
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "TravelDiscountSystem": "Debug"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "logs/app-{Date}.log",
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

### 성능 모니터링
```bash
# CPU 및 메모리 사용량 모니터링
top -p $(pgrep -f "TravelDiscountSystem")

# 애플리케이션 로그 실시간 모니터링
tail -f /var/log/travelapp/app.log

# 데이터베이스 연결 상태 확인
sudo netstat -tuln | grep :1433
```

## 보안 설정

### HTTPS 인증서 설정
```bash
# Let's Encrypt 인증서 발급
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com

# 인증서 자동 갱신 설정
sudo crontab -e
# 다음 라인 추가:
# 0 12 * * * /usr/bin/certbot renew --quiet
```

### 방화벽 설정
```bash
# UFW 방화벽 설정
sudo ufw enable
sudo ufw allow 22/tcp     # SSH
sudo ufw allow 80/tcp     # HTTP
sudo ufw allow 443/tcp    # HTTPS
sudo ufw deny 5000/tcp    # 직접 접근 차단
```

## 백업 전략

### 자동 백업 스크립트
```bash
#!/bin/bash
# 파일: backup.sh

BACKUP_DIR="/backup/travelapp"
DATE=$(date +"%Y%m%d_%H%M%S")

# 디렉토리 생성
mkdir -p $BACKUP_DIR

# 데이터베이스 백업
sqlcmd -S localhost -U sa -P 'YourPassword123!' \
  -Q "BACKUP DATABASE TravelDiscountSystemDb TO DISK = '$BACKUP_DIR/db_$DATE.bak'"

# 애플리케이션 파일 백업
tar -czf $BACKUP_DIR/app_$DATE.tar.gz /var/www/travelapp

# 7일 이상 된 백업 파일 삭제
find $BACKUP_DIR -name "*.bak" -mtime +7 -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +7 -delete

echo "Backup completed: $DATE"
```

### 백업 스케줄 설정
```bash
# crontab 설정
sudo crontab -e

# 매일 새벽 2시에 백업 실행
0 2 * * * /usr/local/bin/backup.sh >> /var/log/backup.log 2>&1
```

## 트러블슈팅

### 일반적인 문제 해결

#### 1. 애플리케이션이 시작되지 않는 경우
```bash
# 로그 확인
sudo journalctl -u travelapp.service -f

# 포트 사용 확인
sudo netstat -tuln | grep :5000

# 권한 확인
ls -la /var/www/travelapp/
```

#### 2. 데이터베이스 연결 오류
```bash
# 데이터베이스 서비스 상태 확인
sudo systemctl status mssql-server

# 연결 테스트
sqlcmd -S localhost -U sa -P 'YourPassword123!'
```

#### 3. 성능 이슈
```bash
# 메모리 사용량 확인
free -h

# 디스크 사용량 확인
df -h

# 데이터베이스 쿼리 성능 분석
-- SQL Server Management Studio에서 실행
SELECT * FROM sys.dm_exec_query_stats
ORDER BY total_elapsed_time DESC
```

## 확장성 고려사항

### 로드 밸런싱
```nginx
# Nginx 로드 밸런서 설정
upstream travelapp {
    server 127.0.0.1:5000;
    server 127.0.0.1:5001;
    server 127.0.0.1:5002;
}

server {
    listen 80;
    location / {
        proxy_pass http://travelapp;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 데이터베이스 최적화
```sql
-- 인덱스 최적화
CREATE NONCLUSTERED INDEX IX_DiscountConditions_Enabled_Type 
ON DiscountConditions (IsEnabled, DiscountType) 
INCLUDE (ConditionNumber, DiscountValue);

CREATE NONCLUSTERED INDEX IX_DiscountCoupons_Status_Dates 
ON DiscountCoupons (IssueStatus, IssueStartDate, IssueEndDate) 
INCLUDE (CouponCode, DiscountRate, DiscountAmount);

-- 통계 업데이트
UPDATE STATISTICS DiscountConditions;
UPDATE STATISTICS DiscountCoupons;
```

## 운영 체크리스트

### 배포 전 확인사항
- [ ] 코드 리뷰 완료
- [ ] 단위 테스트 통과
- [ ] 통합 테스트 통과
- [ ] 보안 스캔 완료
- [ ] 성능 테스트 완료
- [ ] 데이터베이스 마이그레이션 준비
- [ ] 백업 계획 수립
- [ ] 롤백 계획 수립

### 배포 후 확인사항
- [ ] 서비스 정상 구동 확인
- [ ] API 응답 테스트
- [ ] 데이터베이스 연결 확인
- [ ] 로그 모니터링 설정
- [ ] 성능 메트릭 확인
- [ ] 보안 설정 검증
- [ ] 백업 작동 확인

## 문의 및 지원

배포 관련 문제가 발생하면 다음 연락처로 문의하세요:
- 개발팀: dev-team@company.com
- 인프라팀: infra-team@company.com
- 긴급상황: +82-10-1234-5678