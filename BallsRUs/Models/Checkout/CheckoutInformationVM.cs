namespace BallsRUs.Models.Checkout
{
    public class CheckoutInformationVM
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressStateProvince { get; set; }
        public string? AddressCountry { get; set; }
        public string? AddressPostalCode { get; set; }
        public bool HasExistingAddress { get; set; } = false;
        public bool UseExistingAddress { get; set; } = false;
        public bool SaveAddress { get; set; } = false;
        public bool ConfirmInformation { get; set; } = false;
    }
}
