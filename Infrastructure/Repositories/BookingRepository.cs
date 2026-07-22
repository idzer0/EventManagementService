using System.Collections;
using Application.Contracts;
using Domain.DomainExceptions;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

/// <summary>
/// Репозиторий бронирования.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<BookingRepository> _logger;
    private readonly ICurrentUserService _currentUserService;

    public BookingRepository (AppDbContext context, ICurrentUserService currentUserService, ILogger<BookingRepository> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingEntity> CreateBookingAsync(Guid evendId, BookingStatusEnum status, DateTimeOffset createdAt, CancellationToken ct)
    {
        if (!_currentUserService.IsAllowUserOperation(_currentUserService.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        BookingEntity booking = new()
        {
            Id = Guid.NewGuid(),
            EventId = evendId,
            Status = status,
            CreatedAt = createdAt,
            UserId = _currentUserService.UserId.Value,
        };

        await _context.Bookings.AddAsync(booking, ct);
        await _context.SaveChangesAsync(ct);

        return booking;
    }

    /// <inheritdoc/>
    public Task<BookingEntity?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        return _context.Bookings.SingleOrDefaultAsync(b => b.Id == bookingId, ct);
    }

    /// <inheritdoc/>
    public Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct, int num = 10)
    {
        return _context.Bookings.AsNoTracking()
            .Where(b => b.Status == status)
            .OrderBy(b => b.CreatedAt)
            .Take(num)
            .Select(book => book.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task UpdateBookingAsync(BookingEntity entity, CancellationToken ct)
    {
        if (!_currentUserService.IsAllowUserOperation(entity.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        _context.Bookings.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteBookingAsync(BookingEntity entity, CancellationToken ct)
    {
        if (!_currentUserService.IsAllowUserOperation(entity.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        _context.Bookings.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> GetActiveBookingsAsync(CancellationToken ct)
    {
        int? userId = _currentUserService.UserId;

        if (!_currentUserService.IsAllowUserOperation(userId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        return await _context.Bookings
            .CountAsync(b => (b.Status == BookingStatusEnum.Confirmed || b.Status == BookingStatusEnum.Pending)
                            && b.UserId == userId, ct);

    }


}
