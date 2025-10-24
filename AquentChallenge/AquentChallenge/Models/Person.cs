using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquentChallenge.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(50)]
        public string EmailAddress { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string StreetAddress { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(2, MinimumLength = 2, ErrorMessage = "State must be 2 characters.")]
        public string State { get; set; } = string.Empty;

        [Required, StringLength(5, MinimumLength = 5, ErrorMessage = "Zip code must be 5 digits.")]
        public string ZipCode { get; set; } = string.Empty;

        [ForeignKey("Client")]
        public int? ClientId { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
