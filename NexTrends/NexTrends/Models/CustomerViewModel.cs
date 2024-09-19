namespace NexTrends.Models
{
    public class CustomerProfileViewModel
    {
        public Customer Customer { get; set; } = null!;
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Coupon> Coupons { get; set; } = new List<Coupon>();
        public List<Order> PastOrders { get; set; } = new List<Order>();
        public List<Cart> Carts { get; set; } = new List<Cart>();
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
