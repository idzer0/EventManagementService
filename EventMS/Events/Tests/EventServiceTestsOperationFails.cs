using EventMS.Events.Application.Contracts;
using EventMS.Events.Application.DTO;
using EventMS.Events.Application.Services;
using EventMS.Events.Domain.DomainExceptions;
using EventMS.Events.Domain.Models;
using Events.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EventMS.Events.Tests;

[Collection("Неуспешные сценарии")]
public class EventServiceTestsOperationFails
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly IEventService _service;
    private readonly Mock<ICacheService> _cache;
    private readonly Mock<IRedisCacheService> _redis;
    private readonly DbContextMocker _dbContextMocker;

    public EventServiceTestsOperationFails ()
    {
        _dbContextMocker = new DbContextMocker();
        _mockRepository = new Mock<IEventRepository>();
        _cache = new Mock<ICacheService>();
        _redis = new Mock<IRedisCacheService>();
        _service = new EventService(_mockRepository.Object, _cache.Object, NullLogger<EventService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsValidationDomainException()
    {
        var invalidEvent = new EventRequest
        {
            Title = "",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2)
        };

        Func<Task> act = async () => await _service.CreateAsync(invalidEvent, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>()
            .WithMessage("Название события не может быть пустым.");
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<EventEntity>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_InvalidDates_ThrowsValidationDomainException()
    {
        var invalidEvent = new EventRequest
        {
            Title = "Неправильно оформленное событие",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(1) // Дата конца меньше даты начала события
        };

        Func<Task> act = async () => await _service.CreateAsync(invalidEvent, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>()
            .WithMessage("Дата окончания события должна быть больше или равна дате начала.");
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<EventEntity>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsObjectNotFoundDomainException()
    {
        var nonExistentId = Guid.NewGuid();

        _mockRepository.Setup(repo => repo.GetByIdAsync(nonExistentId, CancellationToken.None))
            .ReturnsAsync((EventEntity?)null);

        Func<Task> act = async () => await _service.GetByIdAsync(nonExistentId, CancellationToken.None);

        await act.Should().ThrowAsync<ObjectNotFoundDomainException>()
            .WithMessage($"Событие с Id {nonExistentId} не найдено.");
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonExistentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidDates_ThrowsValidationDomainException()
    {
        var eventId = Guid.NewGuid();
        var existingEvent = new EventEntity
        {
            Id = eventId,
            Title = "Existing",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 100,
            AvailableSeats = 100,
        };

        var invalidUpdate = new EventRequest
        {
            Title = "Invalid",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 100,
        };

        _mockRepository.Setup(repo => repo.GetByIdAsync(eventId, CancellationToken.None))
            .ReturnsAsync(existingEvent);

        Func<Task> act = async () => await _service.UpdateAsync(eventId, invalidUpdate, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationDomainException>()
            .WithMessage("Дата окончания события должна быть больше или равна дате начала.");
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EventEntity>(), CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_MissingEvent_ThrowsObjectNotFoundDomainException()
    {
        var nonExistentEvent = new EventRequest()
        {
            Title = "Несуществующее событие",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1),
            TotalSeats = 100,
        };

        var nonExistentEventId = Guid.NewGuid();

        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Музыкальный фестиваль", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = 100, AvailableSeats = 100 },
            new() { Id = Guid.NewGuid(), Title = "Техническая конференция", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = 100, AvailableSeats = 100 },
            new() { Id = Guid.NewGuid(), Title = "Встреча 1 to 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1), TotalSeats = 100, AvailableSeats = 100 }
        };

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.UpdateAsync_MissingEvent_ThrowsObjectNotFoundDomainException)),
            _redis.Object,
            _cache.Object,
            events);

        Func<Task> act = async () => await service.UpdateAsync(nonExistentEventId, nonExistentEvent, CancellationToken.None);

        await act.Should().ThrowAsync<ObjectNotFoundDomainException>()
            .WithMessage($"Событие с Id {nonExistentEventId} не найдено.");
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid();

        _mockRepository.Setup(repo => repo.DeleteAsync(nonExistentId, CancellationToken.None))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(nonExistentId, CancellationToken.None);

        result.Should().BeFalse();
        _mockRepository.Verify(repo => repo.DeleteAsync(nonExistentId, CancellationToken.None), Times.Once);
    }
}