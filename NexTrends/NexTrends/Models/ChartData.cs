namespace NexTrends.Models
{
    public class ChartData
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public List<ProductTypeSales> SalesByProduct { get; set; }
        public List<DayWiseSales> DayWiseSales { get; set; } // Day-wise sales data
        public List<Category> Categories { get; set; } // Categories for dropdown
        public int SelectedCategoryId { get; set; } // Selected category ID
        public DateTime? StartDate { get; set; } // Start date for filtering
        public DateTime? EndDate { get; set; } // End date for filtering
        public Decimal Baseline { get; set; }
        public int[] SelectedCategoryIds { get; set; }

    }

    public class DayWiseSales
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class ProductTypeSales
    {
        public string ProductType { get; set; }
        public decimal TotalSales { get; set; }
    }
}
