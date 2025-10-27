using System.ComponentModel.DataAnnotations;

namespace AquentChallenge.Models.ViewModels
{
    public class PersonFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s\-']+$", ErrorMessage = "First name cannot contain numbers or special characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s\-']+$", ErrorMessage = "Last name cannot contain numbers or special characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Email must be at most 100 characters.")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Street address must be between 5 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\s.'#,-]+$", ErrorMessage = "Street address contains invalid characters.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z\s\-']+$", ErrorMessage = "City cannot contain numbers or special characters.")]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(2, ErrorMessage = "State must be 2 characters (e.g., FL).")]
        [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "State must be two letters only (e.g., FL).")]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Zip code must be 5 digits or ZIP+4 (e.g., 12345 or 12345-6789).")]
        public string ZipCode { get; set; } = string.Empty;

        public int? ClientId { get; set; }

        // --- UI-Only Fields ---
        public IEnumerable<Client>? Clients { get; set; }
        public string? ClientName { get; set; }
        public bool IsReadOnly { get; set; } = false;
    }
}
