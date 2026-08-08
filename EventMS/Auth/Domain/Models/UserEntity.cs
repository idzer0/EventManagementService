namespace Domain.Models;

using System.ComponentModel.DataAnnotations;
using Auth.Contracts;

public class UserEntity
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля пользователя.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public UsersRole Role { get; set; } = UsersRole.User;

    // Свойство для оптимистической блокировки (маппится на xmin)
    public uint Xmin { get; set; }
}
