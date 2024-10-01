using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retailers.Models
{
    public class User : ITableEntity
    {
        public string PartitionKey { get; set; } = "Users";
        public string RowKey { get; set; } // This will be the username or email

        [Required]
        public string PasswordHash { get; set; } // Store the hashed password

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        // Additional user fields
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}
