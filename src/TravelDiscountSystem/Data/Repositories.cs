using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelDiscountSystem.Domain;

namespace TravelDiscountSystem.Data
{
    // Repository 인터페이스들
    public interface IDiscountConditionRepository
    {
        Task<DiscountCondition> GetByIdAsync(int id);
        Task<List<DiscountCondition>> GetActiveConditionsAsync();
        Task<DiscountCondition> CreateAsync(DiscountCondition condition);
        Task<DiscountCondition> UpdateAsync(DiscountCondition condition);
        Task<bool> DeleteAsync(int id);
    }

    public interface IDiscountCouponRepository
    {
        Task<DiscountCoupon> GetByIdAsync(int id);
        Task<DiscountCoupon> GetByCouponCodeAsync(string couponCode);
        Task<List<DiscountCoupon>> GetActiveCouponsAsync();
        Task<DiscountCoupon> CreateAsync(DiscountCoupon coupon);
        Task<DiscountCoupon> UpdateAsync(DiscountCoupon coupon);
        Task<bool> DeleteAsync(int id);
    }

    public interface IPromotionRepository
    {
        Task<Promotion> GetByIdAsync(int id);
        Task<List<Promotion>> GetActivePromotionsAsync();
        Task<Promotion> CreateAsync(Promotion promotion);
        Task<Promotion> UpdateAsync(Promotion promotion);
        Task<bool> DeleteAsync(int id);
    }

    public interface ICouponUsageHistoryRepository
    {
        Task<CouponUsageHistory> CreateAsync(CouponUsageHistory history);
        Task<List<CouponUsageHistory>> GetByCouponIdAsync(int couponId);
        Task<List<CouponUsageHistory>> GetByUserIdAsync(string userId);
        Task<bool> HasUserUsedCouponAsync(string userId, string couponCode);
    }

    // Repository 구현
    public class DiscountConditionRepository : IDiscountConditionRepository
    {
        private readonly TravelDiscountContext _context;

        public DiscountConditionRepository(TravelDiscountContext context)
        {
            _context = context;
        }

        public async Task<DiscountCondition> GetByIdAsync(int id)
        {
            return await _context.DiscountConditions
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<DiscountCondition>> GetActiveConditionsAsync()
        {
            return await _context.DiscountConditions
                .Where(x => !x.IsDeleted && x.IsEnabled)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<DiscountCondition> CreateAsync(DiscountCondition condition)
        {
            _context.DiscountConditions.Add(condition);
            await _context.SaveChangesAsync();
            return condition;
        }

        public async Task<DiscountCondition> UpdateAsync(DiscountCondition condition)
        {
            condition.UpdatedAt = DateTime.UtcNow;
            _context.DiscountConditions.Update(condition);
            await _context.SaveChangesAsync();
            return condition;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var condition = await GetByIdAsync(id);
            if (condition != null)
            {
                condition.IsDeleted = true;
                condition.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    public class DiscountCouponRepository : IDiscountCouponRepository
    {
        private readonly TravelDiscountContext _context;

        public DiscountCouponRepository(TravelDiscountContext context)
        {
            _context = context;
        }

        public async Task<DiscountCoupon> GetByIdAsync(int id)
        {
            return await _context.DiscountCoupons
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<DiscountCoupon> GetByCouponCodeAsync(string couponCode)
        {
            return await _context.DiscountCoupons
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.CouponCode == couponCode);
        }

        public async Task<List<DiscountCoupon>> GetActiveCouponsAsync()
        {
            var currentDate = DateTime.Now;
            return await _context.DiscountCoupons
                .Where(x => !x.IsDeleted && 
                           x.IssueStatus == "발행중" &&
                           x.IssueStartDate <= currentDate &&
                           x.IssueEndDate >= currentDate)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<DiscountCoupon> CreateAsync(DiscountCoupon coupon)
        {
            _context.DiscountCoupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<DiscountCoupon> UpdateAsync(DiscountCoupon coupon)
        {
            coupon.UpdatedAt = DateTime.UtcNow;
            _context.DiscountCoupons.Update(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coupon = await GetByIdAsync(id);
            if (coupon != null)
            {
                coupon.IsDeleted = true;
                coupon.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    public class PromotionRepository : IPromotionRepository
    {
        private readonly TravelDiscountContext _context;

        public PromotionRepository(TravelDiscountContext context)
        {
            _context = context;
        }

        public async Task<Promotion> GetByIdAsync(int id)
        {
            return await _context.Promotions
                .Include(p => p.DiscountConditions)
                    .ThenInclude(pdc => pdc.DiscountCondition)
                .Include(p => p.DiscountCoupons)
                    .ThenInclude(pdc => pdc.DiscountCoupon)
                .Where(x => !x.IsDeleted)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            var currentDate = DateTime.Now;
            return await _context.Promotions
                .Include(p => p.DiscountConditions)
                    .ThenInclude(pdc => pdc.DiscountCondition)
                .Include(p => p.DiscountCoupons)
                    .ThenInclude(pdc => pdc.DiscountCoupon)
                .Where(x => !x.IsDeleted && 
                           x.Status == "진행중" &&
                           x.StartDate <= currentDate &&
                           x.EndDate >= currentDate)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task<Promotion> CreateAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<Promotion> UpdateAsync(Promotion promotion)
        {
            promotion.UpdatedAt = DateTime.UtcNow;
            _context.Promotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await GetByIdAsync(id);
            if (promotion != null)
            {
                promotion.IsDeleted = true;
                promotion.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    public class CouponUsageHistoryRepository : ICouponUsageHistoryRepository
    {
        private readonly TravelDiscountContext _context;

        public CouponUsageHistoryRepository(TravelDiscountContext context)
        {
            _context = context;
        }

        public async Task<CouponUsageHistory> CreateAsync(CouponUsageHistory history)
        {
            _context.CouponUsageHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<List<CouponUsageHistory>> GetByCouponIdAsync(int couponId)
        {
            return await _context.CouponUsageHistories
                .Where(x => !x.IsDeleted && x.DiscountCouponId == couponId)
                .OrderByDescending(x => x.UsedAt)
                .ToListAsync();
        }

        public async Task<List<CouponUsageHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.CouponUsageHistories
                .Include(x => x.DiscountCoupon)
                .Where(x => !x.IsDeleted && x.UserId == userId)
                .OrderByDescending(x => x.UsedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserUsedCouponAsync(string userId, string couponCode)
        {
            return await _context.CouponUsageHistories
                .Include(x => x.DiscountCoupon)
                .AnyAsync(x => !x.IsDeleted && 
                              x.UserId == userId && 
                              x.DiscountCoupon.CouponCode == couponCode &&
                              x.Status == "사용완료");
        }
    }
}