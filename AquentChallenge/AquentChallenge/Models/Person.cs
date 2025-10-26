using System.ComponentModel.DataAnnotations.Schema;

namespace AquentChallenge.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        // Nullable foreign key: Person may be unassigned to any Client (ClientId == null)
        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
