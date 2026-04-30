using Moq;
using EventManagementService.Contracts;
using EventManagementService.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using EventManagementService.Services;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EventManagementServiceTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Controllers;
using EventManagementService.DomainExceptions;

namespace EventManagementServiceTests;

[Collection("Неуспешные сценарии")]
public class EventServiceTestsOperationFails
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly IEventService _service;
    private readonly DbContextMocker _dbContextMocker;

    public EventServiceTestsOperationFails ()
    {
        _dbContextMocker = new DbContextMocker();
        _mockRepository = new Mock<IEventRepository>();
        _service = new EventService(_mockRepository.Object, NullLogger<EventService>.Instance);
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
            EndAt = DateTime.UtcNow.AddDays(1)
        };

        var invalidUpdate = new EventRequest
        {
            Title = "Invalid",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(1)
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
            { Title = "Несуществующее событие", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) };

        var nonExistentEventId = Guid.NewGuid();

        var events = new List<EventEntity>
        {
            new() { Id = Guid.NewGuid(), Title = "Музыкальный фестиваль", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Техническая конференция", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) },
            new() { Id = Guid.NewGuid(), Title = "Встреча 1 to 1", StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) }
        };

        var service = _dbContextMocker.ArrangeEventServiceTestCase(
            _dbContextMocker.GetAppDbContext(nameof(this.UpdateAsync_MissingEvent_ThrowsObjectNotFoundDomainException)), events);

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