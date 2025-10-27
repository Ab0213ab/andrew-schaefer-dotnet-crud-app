using System.ComponentModel.DataAnnotations;

namespace AquentChallenge.Models.ViewModels
{
    public class ClientFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters.")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Url(ErrorMessage = "Please enter a valid URL (include http:// or https://).")]
        public string? Website { get; set; }

        [StringLength(25, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 25 characters.")]
        [RegularExpression(@"^\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", ErrorMessage = "Phone must be a valid U.S. format (e.g., (555) 555-5555).")]
        public string? Phone { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\s.,#'\-]+$", ErrorMessage = "Address contains invalid characters.")]
        public string? Address { get; set; }

        // UI-specific fields (not in DB)
        public IEnumerable<Person>? AllPeople { get; set; }
        public List<int>? AssociatedIds { get; set; }
        public bool IsReadOnly { get; set; } = false;
    }
}
