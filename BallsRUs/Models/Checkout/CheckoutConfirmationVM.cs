﻿using BallsRUs.Models.Admin;
using FluentValidation;

namespace BallsRUs.Models.Checkout
{
    public class CheckoutConfirmationVM
    {
        public Guid? Id { get; set; }
        public string? Number { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ProductQuantity { get; set; }
        public decimal? ProductCost { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Total { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressStateProvince { get; set; }
        public string? AddressCountry { get; set; }
        public string? AddressPostalCode { get; set; }
        public bool ConfirmInformation { get; set; } = false;
        public bool OrderAlreadyConfirmed { get; set; } = false;
    }

    public class CheckoutConfirmationValidator : AbstractValidator<CheckoutConfirmationVM>
    {
        public CheckoutConfirmationValidator()
        {
            RuleFor(vm => vm.ConfirmInformation)
                .Equal(true)
                    .WithMessage("Veuillez confirmer que les informations sont exactes avant de continuer.");
        }
    }
}