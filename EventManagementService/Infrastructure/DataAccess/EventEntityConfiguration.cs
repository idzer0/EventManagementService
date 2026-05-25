using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventManagementService.Infrastructure.DataAccess;

public class EventEntityConfiguration : IEntityTypeConfiguration<EventEntity>
{
    public void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        // Настройка таблицы
        builder.ToTable("Events");

        // Первичный ключ
        builder.HasKey(e => e.Id);

        // Настройка свойств
        builder.Property(e => e.Id)
            .ValueGeneratedNever() // Id генерируем на среднем слое
            .IsRequired();

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Название события");

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired(false)
            .HasComment("Описание события");

        builder.Property(e => e.StartAt)
            .IsRequired()
            .HasComment("Дата и время начала события");

        builder.Property(e => e.EndAt)
            .IsRequired()
            .HasComment("Дата и время окончания события");

        builder.Property(e => e.TotalSeats)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Общее количество мест");

        builder.Property(e => e.AvailableSeats)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Количество доступных мест");


        #region Индексы

        // Contains с OrdinalIgnoreCase в PostgreSQL превращается в LIKE или ILIKE, который не использует обычный B-Tree индекс
        // Для текстового поиска нужен GIN индекс с gin_trgm_ops (для ILIKE '%text%')
        builder.HasIndex(e => e.Title)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops")
            .HasDatabaseName("IX_Events_Title");

        // Композитный индекс (StartAt, EndAt) эффективен только если фильтруете по обоим полям
        // Если один из фильтров дат часто пропускается, лучше иметь отдельные индексы

        // Индекс для поиска событий по дате начала события
        builder.HasIndex(e => e.StartAt)
            .HasDatabaseName("IX_Events_StartAt");

        // Индекс для поиска событий по дате окончания события
        builder.HasIndex(e => e.EndAt)
            .HasDatabaseName("IX_Events_EndAt");

        #endregion Индексы

        // Настройка связи с BookingEntity (один ко многим)
        builder.HasMany(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict); // Контроль целостности

        // Настройка дополнительных метаданных
        builder.ToTable(t => t.HasComment("Таблица событий"));
    }
}
