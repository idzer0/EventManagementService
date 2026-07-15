
using System.Text.RegularExpressions;
using Application.Contracts;
using Application.DTO;
using FluentValidation;

namespace Application.Services;

public class RequestValidator : AbstractValidator<UserLoginRequest>
{
    private static readonly Regex LoginRegex = new(@"^[a-zA-Z0-9_.-]{3,50}$", RegexOptions.Compiled);
    private static readonly HashSet<string> ReservedLogins = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "root", "system", "administrator"
    };

    public RequestValidator(IUserRepository userRepo)
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Имя пользователя должно быть обязательно")
            .Length(3, 50).WithMessage("Длина имени пользователя от 3 до 50 символов")
            .MustAsync(async (username, ct) => !await userRepo.IsExistsAsync(username, ct))
            .WithMessage("Пользователь с таким именем уже существует")
            .Must((_, login) => LoginRegex.IsMatch(login))
            .WithMessage("Разрешены только латинские буквы, цифры, _, -, .")
            .Must((_, login) => ReservedLogins.Contains(login))
            .WithMessage("Это имя зарезервировано системой")
            .Must((_, login) => login[0] == '.' || login[0] == '_' || login[0] == '-')
            .WithMessage("Имя пользователя не может начинаться с . _ -");


        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .Length(6, 200).WithMessage("Длина пароля от 6 до 200 символов")
            .Must((model, password) => !password.Contains(model.Login, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Пароль не должен содержать имя пользователя");
    }
}
