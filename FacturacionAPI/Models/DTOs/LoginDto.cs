namespace FacturacionAPI.Models.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public EmployeeDto  User { get; set; }
        public bool Success { get; set; }   
    }
}
