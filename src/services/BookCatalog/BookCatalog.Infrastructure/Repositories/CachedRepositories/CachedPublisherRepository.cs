using BookCatalog.Application.Contracts.Repositories;
using BookCatalog.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;

namespace BookCatalog.Infrastructure.Repositories.CachedRepositories;

public class CachedPublisherRepository : CachedRepositoryBase, IPublisherRepository
{
    private readonly IPublisherRepository _publisherRepository;

    public CachedPublisherRepository(
        IPublisherRepository publisherRepository,
        IDistributedCache cache
    ) : base(cache)
    {
        _publisherRepository = publisherRepository;
    }

    public async Task<Publisher> CreateAsync(Publisher publisher, CancellationToken cancellationToken)
    {
        await RemoveCache(GetCacheKey("publishers"), cancellationToken);

        return await _publisherRepository.CreateAsync(publisher, cancellationToken);
    }

    public Task<Publisher?> GetByIdAsync(Guid publisherId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("publisher", publisherId);

        return GetOrSetAsync(
            key,
            () => _publisherRepository.GetByIdAsync(publisherId, cancellationToken),
            cancellationToken
        );
    }

    public async Task<PaginationResult<Publisher>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("publishers", pageNumber, pageSize);

        return await GetOrSetAsync(
            key,
            () => _publisherRepository.GetAsync(pageNumber, pageSize, cancellationToken),
            cancellationToken
        );
    }

    public Task<long> CountAsync(CancellationToken cancellationToken)
    {
        return _publisherRepository.CountAsync(cancellationToken);
    }

    public async Task<Publisher?> UpdateAsync(Guid publisherId, Publisher publisher, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("publisher", publisherId);

        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("publishers"), cancellationToken);

        return await _publisherRepository.UpdateAsync(publisherId, publisher, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid publisherId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("publisher", publisherId);

        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("publishers"), cancellationToken);

        return await _publisherRepository.DeleteAsync(publisherId, cancellationToken);
    }
}