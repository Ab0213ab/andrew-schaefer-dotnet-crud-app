using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquentChallenge.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(50, ErrorMessage = "Email must be at most 50 characters.")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Street address must be between 5 and 100 characters.")]
        public string StreetAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
        public string City { get; set; } = string.Empty;

        [Required]
        // Length set
        [StringLength(2, ErrorMessage = "State must be 2 characters (e.g., FL).")]
        public string State { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must be 5 digits.")]
        public string ZipCode { get; set; } = string.Empty;

        // Nullable foreign key: Person may be unassigned to any Client (ClientId == null)
        [ForeignKey("Client")]
        public int? ClientId { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
