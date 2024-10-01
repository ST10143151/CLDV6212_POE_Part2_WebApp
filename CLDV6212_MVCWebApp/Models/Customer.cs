using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retailers.Models
{
    public class Customer : ITableEntity
    {
        [Key]
        public int Customer_Id { get; set; }  // Ensure this property exists and is populated
        public string? Customer_Name { get; set; }  // Ensure this property exists and is populated
        public string? Email { get; set; }
        public string? Password { get; set; }

        // ITableEntity implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
