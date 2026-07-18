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
        if (!_currentUserService.IsAllowUserOperation())
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
        if (!_currentUserService.IsAllowUserOperation())
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        return _currentUserService.IsAllowAdminOperation() ?
            _context.Bookings.SingleOrDefaultAsync(b => b.Id == bookingId, ct) :
            _context.Bookings.SingleOrDefaultAsync(b => b.Id == bookingId && b.UserId == _currentUserService.UserId.Value, ct);
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
        if (!_currentUserService.IsAllowUserOperation())
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        _context.Bookings.Update(entity);
        await _context.SaveChangesAsync(ct);
    }
}
