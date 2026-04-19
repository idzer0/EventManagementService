using Moq;
using EventManagementService.Contracts;
using EventManagementService.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using EventManagementService.Services;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventManagementServiceTests;

[Collection("Неуспешные сценарии")]
public class EventServiceTestsOperationFails
{
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly IEventService _service;

    public EventServiceTestsOperationFails ()
    {
        _mockRepository = new Mock<IEventRepository>();
        _service = new EventService(_mockRepository.Object, NullLogger<EventService>.Instance);
    }
    
    [Fact]
    public async Task CreateAsync_EmptyTitle_ThrowsArgumentException()
    {
        var invalidEvent = new EventRequest
        {
            Title = "",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2)
        };
        
        Func<Task> act = async () => await _service.CreateAsync(invalidEvent);
        
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Название события не может быть пустым.");
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<EventEntity>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_InvalidDates_ThrowsArgumentException()
    {
        var invalidEvent = new EventRequest
        {
            Title = "Неправильно оформленное событие",
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(1) // Дата конца меньше даты начала события
        };

        Func<Task> act = async () => await _service.CreateAsync(invalidEvent);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Дата окончания события должна быть больше или равна дате начала.");
        _mockRepository.Verify(repo => repo.CreateAsync(It.IsAny<EventEntity>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsKeyNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(nonExistentId))
            .ReturnsAsync((EventEntity?)null);

        Func<Task> act = async () => await _service.GetByIdAsync(nonExistentId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Событие с ID {nonExistentId} не найдено.");
        _mockRepository.Verify(repo => repo.GetByIdAsync(nonExistentId), Times.Once);
    }
      
    [Fact]
    public async Task UpdateAsync_InvalidDates_ThrowsArgumentException()
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
        
        _mockRepository.Setup(repo => repo.GetByIdAsync(eventId))
            .ReturnsAsync(existingEvent);

        Func<Task> act = async () => await _service.UpdateAsync(eventId, invalidUpdate);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Дата окончания события должна быть больше или равна дате начала.");
        _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EventEntity>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var nonExistentId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.DeleteAsync(nonExistentId))
            .ReturnsAsync(false);
        
        var result = await _service.DeleteAsync(nonExistentId);
        
        result.Should().BeFalse();
        _mockRepository.Verify(repo => repo.DeleteAsync(nonExistentId), Times.Once);
    }
}