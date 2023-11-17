using BallsRUs.Utilities;

namespace BallsRUs.Models.ShoppingCart
{
    public class ShoppingCartListVM
    {
        public int? Quantity { get; set; }
        public decimal? ProductsCost { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? SubTotal { get => ProductsCost + ShippingCost; }
        public decimal? Taxes { get => SubTotal * Constants.TAXES_PERCENTAGE; }
        public decimal? Total { get => SubTotal + Taxes; }
        public List<ShoppingCartProductVM>? Items { get; set; }
    }
}
