using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess;

public class BookingEntityConfiguration : IEntityTypeConfiguration<BookingEntity>
{
    public void Configure(EntityTypeBuilder<BookingEntity> builder)
    {
        // Настройка таблицы
        builder.ToTable("bookings");

        // Первичный ключ
        builder.HasKey(b => b.Id);

        // Настройка свойств
        builder.Property(b => b.Id)
            .ValueGeneratedNever() // Id генерируем на среднем слое
            .HasComment("Уникальный идентификатор брони")
            .IsRequired();

        builder.Property(b => b.EventId)
            .HasComment("Идентификатор события, к которому относится бронь")
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasComment("Текущий статус брони (1=Pending, 2=Confirmed, 3=Rejected)")
            .HasConversion<int>()
            .HasDefaultValue(BookingStatusEnum.Pending)
            .HasSentinel(0);

        builder.Property(b => b.CreatedAt)
            .HasComment("Дата и время создания брони")
            .IsRequired();

        builder.Property(b => b.ProcessedAt)
            .HasComment("Дата и время обработки брони (подтверждение/отмена/истечение)")
            .IsRequired(false);

        // Индекс для поиска по статусу и сортировки по дате создания, для метода GetBookingIdsByStatusAsync
        builder.HasIndex(b => new { b.Status, b.CreatedAt });

        // Комментарии к таблице и колонкам
        builder.ToTable(t => t.HasComment("Таблица бронирований мест на события"));
    }
}
