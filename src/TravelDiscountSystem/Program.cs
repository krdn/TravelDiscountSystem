using Microsoft.EntityFrameworkCore;
using TravelDiscountSystem.Data;
using TravelDiscountSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TravelDiscountContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository 등록
builder.Services.AddScoped<IDiscountConditionRepository, DiscountConditionRepository>();
builder.Services.AddScoped<IDiscountCouponRepository, DiscountCouponRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<ICouponUsageHistoryRepository, CouponUsageHistoryRepository>();

// Service 등록
builder.Services.AddScoped<IDiscountService, DiscountService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "여행상품 할인/쿠폰 관리 API",
        Version = "v1",
        Description = "여행상품의 프로모션, 할인조건, 할인쿠폰을 관리하고 할인 금액을 계산하는 API"
    });
});

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelDiscountSystem v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();