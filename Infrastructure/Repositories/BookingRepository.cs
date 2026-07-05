using System.Collections;
using Application.Contracts;
using Infrastructure.DataAccess;
using Domain.Models;
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

    public BookingRepository (AppDbContext context, ILogger<BookingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingEntity> CreateBookingAsync(Guid evendId, BookingStatusEnum status, DateTimeOffset createdAt, CancellationToken ct)
    {
        BookingEntity booking = new()
            {
                Id = Guid.NewGuid(),
                EventId = evendId,
                Status = status,
                CreatedAt = createdAt
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
}
