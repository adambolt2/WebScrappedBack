using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrappedBack.Models.Entities
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        [MaxLength(128)]
        public string ApiKey { get; set; }

        [Required]
        public bool IndeedSubscribed { get; set; } = false;

        [Required]
        public bool LinkedInSubscribed { get; set; } = false;

        [Required]
        public bool TotalJobsSubscribed { get; set; } = false;

        [Required]
        public bool isVerified { get; set; } = false;

        [MaxLength(50)]
        public string VerificationCode { get; set; }

        public Users()
        {
            ApiKey = GenerateApiKey();
            VerificationCode = GenerateVerificationCode();
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateVerificationCode()
        {
            // Generate a unique verification code, you can adjust the format as needed
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
