using System.ComponentModel.DataAnnotations;

namespace AquentChallenge.Models.ViewModels
{
    public class PersonFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(50, ErrorMessage = "Email must be at most 50 characters.")]
        public string EmailAddress { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 5, ErrorMessage = "Street address must be between 5 and 100 characters.")]
        public string StreetAddress { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
        public string City { get; set; } = string.Empty;

        [StringLength(2, ErrorMessage = "State must be 2 characters (e.g., FL).")]
        public string State { get; set; } = string.Empty;

        [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must be 5 digits.")]
        public string ZipCode { get; set; } = string.Empty;

        public int? ClientId { get; set; }

        // --- UI-Only Fields ---
        public IEnumerable<Client>? Clients { get; set; }
        public string? ClientName { get; set; }
        public bool IsReadOnly { get; set; } = false;
    }
}
