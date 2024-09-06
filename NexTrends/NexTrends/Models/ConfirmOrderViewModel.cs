using System;

namespace NexTrends.Models
{
    public class ConfirmOrderViewModel
    {
        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public int NumberOfProducts { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
