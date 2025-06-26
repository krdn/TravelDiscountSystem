using Microsoft.EntityFrameworkCore;
using TravelDiscountSystem.Domain;

namespace TravelDiscountSystem.Data
{
    public class TravelDiscountContext : DbContext
    {
        public TravelDiscountContext(DbContextOptions<TravelDiscountContext> options) : base(options)
        {
        }

        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<DiscountCondition> DiscountConditions { get; set; }
        public DbSet<DiscountCoupon> DiscountCoupons { get; set; }
        public DbSet<PromotionDiscountCondition> PromotionDiscountConditions { get; set; }
        public DbSet<PromotionDiscountCoupon> PromotionDiscountCoupons { get; set; }
        public DbSet<CouponUsageHistory> CouponUsageHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Promotion 엔터티 구성
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PromotionNumber).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.ProposalNumber).HasMaxLength(50);
                entity.Property(e => e.BudgetAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SupportAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PrefixCode).HasMaxLength(20);
                entity.Property(e => e.ProductCategory).HasMaxLength(50);
                entity.Property(e => e.DepartureLocation).HasMaxLength(100);
                entity.Property(e => e.ApplicableCountries).HasMaxLength(500);
                entity.Property(e => e.ExcludedCountries).HasMaxLength(500);
                entity.Property(e => e.ApplicableCities).HasMaxLength(500);
                entity.Property(e => e.ExcludedCities).HasMaxLength(500);
                entity.Property(e => e.Airlines).HasMaxLength(500);
                
                entity.HasIndex(e => e.PromotionNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });
            });

            // DiscountCondition 엔터티 구성
            modelBuilder.Entity<DiscountCondition>(entity =>
            {
                entity.ToTable("DiscountConditions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ConditionNumber).IsRequired();
                entity.Property(e => e.DiscountType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.AmountType).HasMaxLength(20);
                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,6)");
                entity.Property(e => e.MinimumAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaximumDiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApplicableTarget).HasMaxLength(50);
                entity.Property(e => e.GenderCondition).HasMaxLength(20);
                entity.Property(e => e.GroupNumber).HasMaxLength(50);
                entity.Property(e => e.EstimatedDiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EstimatedSellingPrice).HasColumnType("decimal(18,2)");
                
                entity.HasIndex(e => e.ConditionNumber).IsUnique();
                entity.HasIndex(e => e.DiscountType);
                entity.HasIndex(e => e.IsEnabled);
            });

            // DiscountCoupon 엔터티 구성
            modelBuilder.Entity<DiscountCoupon>(entity =>
            {
                entity.ToTable("DiscountCoupons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CouponCode).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IssueStatus).HasMaxLength(20);
                entity.Property(e => e.IssueType).HasMaxLength(50);
                entity.Property(e => e.CouponName).HasMaxLength(200);
                entity.Property(e => e.TargetProducts).HasMaxLength(100);
                entity.Property(e => e.PackageType).HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.AppType).HasMaxLength(20);
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(5,4)");
                entity.Property(e => e.MinimumAmountForRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MaximumDiscountForRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinimumAmountForFixed).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApplicableCountries).HasMaxLength(500);
                entity.Property(e => e.ExcludedCountries).HasMaxLength(500);
                entity.Property(e => e.ApplicableCities).HasMaxLength(500);
                entity.Property(e => e.ExcludedCities).HasMaxLength(500);
                entity.Property(e => e.ProductConditions).HasMaxLength(500);
                entity.Property(e => e.HotelConditions).HasMaxLength(500);
                entity.Property(e => e.AirlineConditions).HasMaxLength(500);
                entity.Property(e => e.LandConditions).HasMaxLength(500);
                entity.Property(e => e.BudgetSupportAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PromotionIssueAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ApplicableProductCodes).HasMaxLength(1000);
                entity.Property(e => e.Remarks).HasMaxLength(1000);
                
                entity.HasIndex(e => e.CouponCode).IsUnique();
                entity.HasIndex(e => e.IssueStatus);
                entity.HasIndex(e => new { e.IssueStartDate, e.IssueEndDate });
            });

            // PromotionDiscountCondition 연결 테이블 구성
            modelBuilder.Entity<PromotionDiscountCondition>(entity =>
            {
                entity.ToTable("PromotionDiscountConditions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Priority).HasDefaultValue(1);
                
                entity.HasOne(e => e.Promotion)
                    .WithMany(p => p.DiscountConditions)
                    .HasForeignKey(e => e.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.DiscountCondition)
                    .WithMany(d => d.PromotionConditions)
                    .HasForeignKey(e => e.DiscountConditionId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => new { e.PromotionId, e.DiscountConditionId }).IsUnique();
            });

            // PromotionDiscountCoupon 연결 테이블 구성
            modelBuilder.Entity<PromotionDiscountCoupon>(entity =>
            {
                entity.ToTable("PromotionDiscountCoupons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Priority).HasDefaultValue(1);
                
                entity.HasOne(e => e.Promotion)
                    .WithMany(p => p.DiscountCoupons)
                    .HasForeignKey(e => e.PromotionId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.DiscountCoupon)
                    .WithMany(d => d.PromotionCoupons)
                    .HasForeignKey(e => e.DiscountCouponId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => new { e.PromotionId, e.DiscountCouponId }).IsUnique();
            });

            // CouponUsageHistory 엔터티 구성
            modelBuilder.Entity<CouponUsageHistory>(entity =>
            {
                entity.ToTable("CouponUsageHistories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProductCode).HasMaxLength(50).IsRequired();
                entity.Property(e => e.OriginalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.OrderNumber).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
                
                entity.HasOne(e => e.DiscountCoupon)
                    .WithMany(d => d.UsageHistories)
                    .HasForeignKey(e => e.DiscountCouponId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ProductCode);
                entity.HasIndex(e => e.OrderNumber);
                entity.HasIndex(e => e.UsedAt);
            });

            // 공통 필드 설정
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Property("CreatedAt")
                        .HasDefaultValueSql("GETUTCDATE()");
                    modelBuilder.Entity(entityType.ClrType).Property("IsDeleted")
                        .HasDefaultValue(false);
                    modelBuilder.Entity(entityType.ClrType).HasIndex("IsDeleted");
                }
            }
        }
    }
}