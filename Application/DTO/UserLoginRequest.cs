namespace Application.DTO;

public class UserLoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Role { get; set; } = 1;
}
