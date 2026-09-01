namespace VibeCheck.Api.DTOs
{
    public class AuthResultDTO
    {
        public bool Succeeded { get; set; }

        public AuthResponseDTO? Response { get; set; }

        public IEnumerable<string> Errors { get; set; } = [];
    }
}
