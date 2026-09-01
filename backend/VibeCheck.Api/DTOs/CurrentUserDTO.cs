namespace VibeCheck.Api.DTOs
{
    public class CurrentUserDTO
    {
        public string UserName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = [];
    }
}
