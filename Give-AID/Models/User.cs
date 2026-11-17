

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
    {
        public int Id { get; set; }
         public required string  FullName { get; set; }

        // Unique index AppDBContext mein configure hai
        [StringLength(100)]
        [EmailAddress]
        public required string  Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Occupation { get; set; }
        public string? Organisation { get; set; }
         public string? Role { get; set; }
        public required string  Password { get; set; }
        [NotMapped]public required string  ConfirmPassword { get; set; }
         public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

