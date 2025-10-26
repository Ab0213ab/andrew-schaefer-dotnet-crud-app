using System.ComponentModel.DataAnnotations;

namespace AquentChallenge.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters.")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Url(ErrorMessage = "Please enter a valid URL (include http:// or https://).")]
        public string? Website { get; set; }

        [StringLength(25, MinimumLength = 10)]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [RegularExpression(@"^\+?[\d\-\s\(\)]{7,25}$", ErrorMessage = "Please enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be at least 5 characters.")]
        public string? Address { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
