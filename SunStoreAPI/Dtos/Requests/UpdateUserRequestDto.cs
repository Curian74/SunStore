using System.ComponentModel.DataAnnotations;

namespace SunStoreAPI.Dtos.Requests
{
    public class UpdateUserRequestDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string? Username { get; set; }

        public string? Address { get; set; }

        public DateOnly BirthDate { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; } = null!;

        public int? Role { get; set; }

        public int? IsBanned { get; set; }
    }
}
