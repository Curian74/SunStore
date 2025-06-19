namespace SunStore.ViewModel.DataModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? Address { get; set; }

        public DateOnly BirthDate { get; set; }

        public string? Email { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public int Role { get; set; }

        public int? IsBanned { get; set; }
    }
}
