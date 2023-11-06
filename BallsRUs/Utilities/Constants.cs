namespace BallsRUs.Utilities
{
    public static class Constants
    {
        // Nombre d'occurences
        public const int NB_OF_SHOWCASED_PRODUCTS = 4;

        // Types de tri
        public const string PRICE_HIGH_TO_LOW = "high-to-low";
        public const string PRICE_LOW_TO_HIGH = "low-to-high";
        public const string BRAND_ALPHABETICAL = "brand-alphabetical";
        public const string RELEASE_NEW_TO_OLD = "new-to-old";

        // Pour les ViewModels des produits
        public const int PRODUCT_MIN_NAME_LENGTH = 5;
        public const int PRODUCT_MAX_NAME_LENGTH = 100;
        public const int PRODUCT_MIN_SHORT_DESC_LENGTH = 20;
        public const int PRODUCT_MAX_SHORT_DESC_LENGTH = 200;
        public const int PRODUCT_MIN_FULL_DESC_LENGTH = 100;
        public const int PRODUCT_MAX_FULL_DESC_LENGTH = 1200;
        public const string PRODUCT_SKU_REGEX = "^[A-Z]{3}\\d{4}[A-Z]{3}$";
    }
}
