using BallsRUs.Entities;
using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Account
{
    public class OrderManageVM
    {
        [Display(Name = "Id")]
        public Guid Id { get; set; }

        [Display(Name = "Order#")]
        public string? Number { get; set; }

        [Display(Name = "Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Opened;
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [Display(Name = "Email")]
        public string? EmailAddress { get; set; }
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Quantity")]
        public int ProductQuantity { get; set; }
        [Display(Name = "Product price")]
        public decimal? ProductsCost { get; set; }
        [Display(Name = "Shipping fees")]
        public decimal? ShippingCost { get; set; }
        [Display(Name = "Sub total")]
        public decimal? SubTotal { get; set; }
        [Display(Name = "Taxes")]
        public decimal? Taxes { get; set; }
        [Display(Name = "Total")]
        public decimal? Total { get; set; }
        [Display(Name = "Creation date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Confirmation date")]
        public DateTime? ConfirmationDate { get; set; }
        [Display(Name = "Purchased on")]
        public DateTime? PaymentDate { get; set; }
        [Display(Name = "Modified on")]
        public DateTime? ModificationDate { get; set; }

        public virtual User? User { get; set; }
    }
}