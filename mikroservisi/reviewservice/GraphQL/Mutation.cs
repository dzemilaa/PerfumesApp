using Microsoft.EntityFrameworkCore;
using HotChocolate;
using reviewservice.Data;
using reviewservice.Models;
using reviewservice.Services;

namespace reviewservice.GraphQL
{
    public class Mutation
    {
        public async Task<ReviewResult> AddReview(
            [GraphQLNonNullType] string userId,
            [GraphQLNonNullType] string orderId,
            [GraphQLNonNullType] string productId,
            [GraphQLNonNullType] string reviewText,
            [Service] ReviewsDbContext dbContext,
            [Service] IOrderServiceClient orderClient)
        {
            if (!Guid.TryParse(userId, out var userGuid))
                return new ReviewResult(false, "Invalid userId.", null);

            if (!Guid.TryParse(orderId, out var orderGuid))
                return new ReviewResult(false, "Invalid orderId.", null);

            if (!Guid.TryParse(productId, out var productGuid))
                return new ReviewResult(false, "Invalid productId.", null);

            if (string.IsNullOrWhiteSpace(reviewText))
                return new ReviewResult(false, "Review text cannot be empty.", null);

            var isValid = await orderClient.ValidateOrderAndProduct(userGuid, orderGuid, productGuid);
            if (!isValid)
                return new ReviewResult(false, "Order or product not found for this user.", null);

            var existing = await dbContext.Reviews.FirstOrDefaultAsync(r =>
                r.UserId == userGuid &&
                r.OrderId == orderGuid &&
                r.ProductId == productGuid);

            if (existing != null)
                return new ReviewResult(false, "You have already reviewed this product for this order.", null);

            var review = new Reviews
            {
                ReviewId = Guid.NewGuid(),
                UserId = userGuid,
                OrderId = orderGuid,
                ProductId = productGuid,
                ReviewText = reviewText,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Reviews.Add(review);
            await dbContext.SaveChangesAsync();

            return new ReviewResult(true, "Review added successfully.", MapToReviewView(review));
        }

        public async Task<ReviewResult> DeleteReview(
      [GraphQLNonNullType] string reviewId,
      [Service] ReviewsDbContext dbContext)
        {
            if (!Guid.TryParse(reviewId, out var reviewGuid))
                return new ReviewResult(false, "Invalid reviewId.", null);

            var review = await dbContext.Reviews.FirstOrDefaultAsync(r =>
                r.ReviewId == reviewGuid);

            if (review == null)
                return new ReviewResult(false, "Review not found.", null);

            dbContext.Reviews.Remove(review);
            await dbContext.SaveChangesAsync();

            return new ReviewResult(true, "Review deleted successfully.", null);
        }

        private ReviewView MapToReviewView(Reviews review)
        {
            return new ReviewView(
                review.ReviewId,
                review.UserId,
                review.OrderId,
                review.ProductId,
                review.ReviewText,
                review.CreatedAt
            );
        }
    }

    public class ReviewResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReviewView? Review { get; set; }

        public ReviewResult() { }
        public ReviewResult(bool success, string message, ReviewView? review)
        {
            Success = success;
            Message = message;
            Review = review;
        }
    }

    public class ReviewView
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ReviewText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ReviewView(Guid reviewId, Guid userId, Guid orderId, Guid productId, string reviewText, DateTime createdAt)
        {
            ReviewId = reviewId;
            UserId = userId;
            OrderId = orderId;
            ProductId = productId;
            ReviewText = reviewText;
            CreatedAt = createdAt;
        }
    }
}