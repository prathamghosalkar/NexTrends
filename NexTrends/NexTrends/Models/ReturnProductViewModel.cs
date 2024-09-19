namespace NexTrends.Models
{
    public class ReturnProductViewModel
    {
        public int ProductId { get; set; }
        public int Product_Q { get; set; }
        public string ProductName { get; set; }
        public Order Order { get; set; }
        public decimal PricePerUnit { get; set; }
        public int QuantityToReturn { get; set; } // Ensure to bind this in the form
        public string Reasons { get; set; } // Captures the reason for return
        public string AdditionalD { get; set; } // Captures the additional details
        public decimal TotalAmount { get; set; }
    }
}
