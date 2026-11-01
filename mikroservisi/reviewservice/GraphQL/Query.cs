using HotChocolate;
using Microsoft.EntityFrameworkCore;
using reviewservice.Data;

namespace reviewservice.GraphQL
{
    public class Query
    {
        public async Task<List<ReviewView>> GetReviewsByProduct(
            [GraphQLNonNullType] string productId,
            [Service] ReviewsDbContext dbContext)
        {
            if (!Guid.TryParse(productId, out var productGuid))
                return new List<ReviewView>();

            var reviews = await dbContext.Reviews
                .Where(r => r.ProductId == productGuid)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new ReviewView(
                r.ReviewId,
                r.UserId,
                r.OrderId,
                r.ProductId,
                r.ReviewText,
                r.CreatedAt
            )).ToList();
        }
        public async Task<List<ReviewView>> GetAllReviews(
    [Service] ReviewsDbContext dbContext)
        {
            var reviews = await dbContext.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new ReviewView(
                r.ReviewId,
                r.UserId,
                r.OrderId,
                r.ProductId,
                r.ReviewText,
                r.CreatedAt
            )).ToList();
        }
        public async Task<List<ReviewView>> GetReviewsByUser(
            [GraphQLNonNullType] string userId,
            [Service] ReviewsDbContext dbContext)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return new List<ReviewView>();

            var reviews = await dbContext.Reviews
                .Where(r => r.UserId == userGuid)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new ReviewView(
                r.ReviewId,
                r.UserId,
                r.OrderId,
                r.ProductId,
                r.ReviewText,
                r.CreatedAt
            )).ToList();
        }
    }
}