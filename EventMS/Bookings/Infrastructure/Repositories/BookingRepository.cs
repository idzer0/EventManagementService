using System.Collections;
using EventMS.Bookings.Application.Contracts;
using EventMS.Bookings.Domain.DomainExceptions;
using EventMS.Bookings.Domain.Models;
using EventMS.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventMS.Bookings.Infrastructure.Repositories;

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
    public async Task<BookingEntity> CreateBookingAsync(Guid evendId, int userId, BookingStatusEnum status, DateTimeOffset createdAt, CancellationToken ct)
    {
        BookingEntity booking = new()
        {
            Id = Guid.NewGuid(),
            EventId = evendId,
            Status = status,
            CreatedAt = createdAt,
            UserId = userId,
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
        _context.Bookings.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task DeleteBookingAsync(BookingEntity entity, CancellationToken ct)
    {
        _context.Bookings.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public Task<int> GetActiveBookingsAsync(int userId, CancellationToken ct)
    {
        return _context.Bookings
            .CountAsync(b => (b.Status == BookingStatusEnum.Confirmed || b.Status == BookingStatusEnum.Pending)
                            && b.UserId == userId, ct);

    }


}
