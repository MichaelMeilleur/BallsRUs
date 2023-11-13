namespace BallsRUs.Models.ShoppingCart
{
    public class ShoppingCartListVM
    {
        public int? Quantity { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Total { get; set; }
        public List<ShoppingCartProductVM>? Items { get; set; }
    }
}
