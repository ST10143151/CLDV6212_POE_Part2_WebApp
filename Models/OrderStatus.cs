using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace ABC_Retailers.Models
{
    public class OrderStatus : ITableEntity
    {
        [Key]
        public int OrderStatus_Id { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        //Introduce validation sample
        [Required(ErrorMessage = "Please select a birder.")]
        public int Customer_ID { get; set; } // FK to the Birder who made the sighting

        [Required(ErrorMessage = "Please select a bird.")]
        public int Product_ID { get; set; } // FK to the Bird being sighted

        [Required(ErrorMessage = "Please select the date.")]
        public DateTime OrderStatus_Date { get; set; } 

        [Required(ErrorMessage = "Please enter the location.")]
        public string? OrderStatus_Location { get; set; } 
    }
}
