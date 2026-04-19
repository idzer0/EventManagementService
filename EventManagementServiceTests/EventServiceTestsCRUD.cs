using Moq;
using EventManagementService.Contracts;
using EventManagementService.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using EventManagementService.Services;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests;

[Collection("Успешные сценарии CRUD")]
public class EventServiceTestsCRUD
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly IEventService _service;

    public EventServiceTestsCRUD ()
    {
        _mockRepository = new Mock<IEventRepository>();
        _service = new EventService(_mockRepository.Object, NullLogger<EventService>.Instance);
    }
    
    [Fact]
    public async Task CreateAsync_ValidEvent_ReturnsEventWithId()
    {
        var newEvent = new EventRequest
        {
            Title = "Проверочное событие",
            Description = "Описание проверочного события",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
        };
        
        var createdEvent = new EventEntity
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
        };
        
        _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<EventEntity>()))
            .ReturnsAsync(createdEvent);
        
        var result = await _service.CreateAsync(newEvent);
        
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be(newEvent.Title);
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<EventEntity>()), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEvent()
    {
        var eventId = Guid.NewGuid();
        var expectedEvent = new EventEntity
        {
            Id = eventId,
            Title = "Существующее событие",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(eventId))
            .ReturnsAsync(expectedEvent);
        
        var result = await _service.GetByIdAsync(eventId);
        
        result.Should().NotBeNull();
        result.Id.Should().Be(eventId);
        result.Title.Should().Be("Существующее событие");
        _mockRepository.Verify(repo => repo.GetByIdAsync(eventId), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_ValidEvent_ReturnsUpdatedEvent()
    {
        var eventId = Guid.NewGuid();
        var existingEvent = new EventEntity
        {
            Id = eventId,
            Title = "Устаревшее наименование",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(1)
        };
        
        var updatedEvent = new EventRequest
        {
            Title = "Новое наименование",
            Description = "Обновленное описание",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddDays(2),
        };
        
        _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<EventEntity>()))
            .ReturnsAsync(existingEvent);
        
        var result = await _service.UpdateAsync(eventId, updatedEvent);
        
        result.Should().NotBeNull();
        result.Title.Should().Be("Новое наименование");
        result.Description.Should().Be("Обновленное описание");
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EventEntity>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        var eventId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.DeleteAsync(eventId))
            .ReturnsAsync(true);
        
        var result = await _service.DeleteAsync(eventId);
        
        result.Should().BeTrue();
        _mockRepository.Verify(repo => repo.DeleteAsync(eventId), Times.Once);
    }
}