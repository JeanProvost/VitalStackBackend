using Backend.Core.Entities.Base;

namespace Backend.Core.Entities.Users
{
    public class User : BaseEntity<Guid>
    {
        public required string IdentityId { get; set; }
        public required string Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public long? PhoneNumber { get; set; }
    }
}
