namespace Application.Auth.Dtos
{
    public class LoginDto
    {
        public string EmailOrUsername { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}