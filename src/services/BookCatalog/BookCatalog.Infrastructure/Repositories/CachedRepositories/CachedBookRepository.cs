using BookCatalog.Application.Contracts.Repositories;
using BookCatalog.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Shared.DTOs;

namespace BookCatalog.Infrastructure.Repositories.CachedRepositories;

public class CachedBookRepository : CachedRepositoryBase, IBookRepository
{
    private readonly IBookRepository _bookRepository;

    public CachedBookRepository(
        IBookRepository bookRepository,
        IDistributedCache cache
    ) : base(cache)
    {
        _bookRepository = bookRepository;
    }

    public Task<Book> CreateAsync(Book book, CancellationToken cancellationToken)
    {
        var listKeyPrefix = GetCacheKey("books");
        _ = RemoveCache(listKeyPrefix, cancellationToken);
        return _bookRepository.CreateAsync(book, cancellationToken);
    }

    public Task<Book?> GetByIdAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("book", bookId);

        return GetOrSetAsync(
            key,
            () => _bookRepository.GetByIdAsync(bookId, cancellationToken),
            cancellationToken
        );
    }

    public Task<long> CountAsync(CancellationToken cancellationToken)
    {
        return _bookRepository.CountAsync(cancellationToken);
    }

    public async Task<PaginationResult<Book>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("books", pageNumber, pageSize);

        return await GetOrSetAsync<PaginationResult<Book>>(
            key,
            () => _bookRepository.GetAsync(pageNumber, pageSize, cancellationToken),
            cancellationToken
        );
    }

    public async Task<Book?> UpdateAsync(Guid bookId, Book book, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("book", bookId);

        await RemoveCache(key, cancellationToken);
        
        var listKeyPrefix = GetCacheKey("books");
        await RemoveCache(listKeyPrefix, cancellationToken);

        return await _bookRepository.UpdateAsync(bookId, book, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var key = GetCacheKey("book", bookId);
        await RemoveCache(key, cancellationToken);

        var listKeyPrefix = GetCacheKey("books");
        await RemoveCache(listKeyPrefix, cancellationToken);

        return await _bookRepository.DeleteAsync(bookId, cancellationToken);
    }
}