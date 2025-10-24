using System.ComponentModel.DataAnnotations;

namespace AquentChallenge.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Website { get; set; }

        [StringLength(25)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
