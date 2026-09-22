using EventMS.Auth.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventMS.Auth.Infrastructure.DataAccess;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Настройка таблицы
        builder.ToTable("users");

        // Первичный ключ
        builder.HasKey(e => e.Id);

        // Настройка свойств
        builder.Property(e => e.Id)
            .UseIdentityByDefaultColumn()
            .IsRequired();

        builder.Property(e => e.Login)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Логин пользователя");

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .IsRequired()
            .HasComment("Хеш пароля пользователя");

        builder.Property(e => e.Role)
            .IsRequired()
            .HasComment("Идентификатор роли пользователя");

        // Указываем, что Xmin маппится на системный столбец xmin
        builder.Property(e => e.Xmin)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        #region Индексы

        builder.HasIndex(e => e.Login)
            .IsUnique()
            .HasDatabaseName("IX_Users_Login");

        #endregion Индексы

        // Настройка дополнительных метаданных
        builder.ToTable(t => t.HasComment("Таблица пользователей"));
    }
}
