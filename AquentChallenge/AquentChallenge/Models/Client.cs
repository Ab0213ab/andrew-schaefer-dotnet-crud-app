
namespace AquentChallenge.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
