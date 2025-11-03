namespace Application.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
