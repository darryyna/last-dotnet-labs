using BookCatalog.Application.Contracts.Repositories;
using BookCatalog.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;

namespace BookCatalog.Infrastructure.Repositories.CachedRepositories;

public class CachedGenreRepository : CachedRepositoryBase, IGenreRepository
{
    private readonly IGenreRepository _genreRepository;

    public CachedGenreRepository(
        IGenreRepository genreRepository,
        IDistributedCache cache
    ) : base(cache)
    {
        _genreRepository = genreRepository;
    }

    public async Task<Genre> CreateAsync(Genre genre, CancellationToken cancellationToken)
    {
        await RemoveCache(GetCacheKey("genres"), cancellationToken);

        return await _genreRepository.CreateAsync(genre, cancellationToken);
    }

    public Task<Genre?> GetByIdAsync(Guid genreId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("genre", genreId);

        return GetOrSetAsync(
            key,
            () => _genreRepository.GetByIdAsync(genreId, cancellationToken),
            cancellationToken
        );
    }

    public async Task<PaginationResult<Genre>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("genres", pageNumber, pageSize);

        return await GetOrSetAsync(
            key,
            () => _genreRepository.GetAsync(pageNumber, pageSize, cancellationToken),
            cancellationToken
        );
    }

    public Task<long> CountAsync(CancellationToken cancellationToken)
    {
        return _genreRepository.CountAsync(cancellationToken);
    }

    public async Task<Genre?> UpdateAsync(Guid genreId, Genre genre, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("genre", genreId);

        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("genres"), cancellationToken);

        return await _genreRepository.UpdateAsync(genreId, genre, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid genreId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("genre", genreId);

        await RemoveCache(key, cancellationToken);
        await RemoveCache(GetCacheKey("genres"), cancellationToken);

        return await _genreRepository.DeleteAsync(genreId, cancellationToken);
    }
}
