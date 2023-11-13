namespace BallsRUs.Models.ShoppingCart
{
    public class ShoppingCartProductVM
    {
        public string? Name { get; set; }
        public decimal? RetailPrice { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public string? ImagePath { get; set; }
        public int? Quantity { get; set; }
    }
}
