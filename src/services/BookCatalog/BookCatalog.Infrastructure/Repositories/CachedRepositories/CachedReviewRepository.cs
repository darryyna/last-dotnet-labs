using System.Linq.Expressions;
using BookCatalog.Application.Contracts.Repositories;
using BookCatalog.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;

namespace BookCatalog.Infrastructure.Repositories.CachedRepositories;

public class CachedReviewRepository : CachedRepositoryBase, IReviewRepository
{
    private readonly IReviewRepository _reviewRepository;

    public CachedReviewRepository(IReviewRepository reviewRepository, IDistributedCache cache)
        : base(cache)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Review> CreateAsync(Review review, CancellationToken cancellationToken)
    {
        await RemoveCache(GetCacheKey("reviews", review.BookId), cancellationToken);

        return await _reviewRepository.CreateAsync(review, cancellationToken);
    }

    public Task<Review?> GetByIdAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("review", reviewId);

        return GetOrSetAsync(
            key,
            () => _reviewRepository.GetByIdAsync(reviewId, cancellationToken),
            cancellationToken
        );
    }

    public async Task<PaginationResult<Review>> GetAsync(Guid bookId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("reviews", bookId, pageNumber, pageSize);

        return await GetOrSetAsync(
            key,
            () => _reviewRepository.GetAsync(bookId, pageNumber, pageSize, cancellationToken),
            cancellationToken
        );
    }

    public Task<long> CountAsync(Expression<Func<Review, bool>> predicate, CancellationToken cancellationToken)
    {
        return _reviewRepository.CountAsync(predicate, cancellationToken);
    }

    public async Task<Review?> UpdateAsync(Guid reviewId, Review review, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("review", reviewId);

        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("reviews", review.BookId), cancellationToken);

        return await _reviewRepository.UpdateAsync(reviewId, review, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid reviewId, CancellationToken cancellationToken)
    {
        var existingReview = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

        if (existingReview == null)
            return false;

        var key = GetCacheKey("review", reviewId);
        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("reviews", existingReview.BookId), cancellationToken);

        return await _reviewRepository.DeleteAsync(reviewId, cancellationToken);
    }
}