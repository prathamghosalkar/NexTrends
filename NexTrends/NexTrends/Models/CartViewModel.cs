namespace NexTrends.Models
{
    public class CartViewModel
    {
        public List<Cart> CartItems { get; set; }
        public List<Coupon> AvailableCoupons { get; set; }

        public int? AppliedCouponId { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
