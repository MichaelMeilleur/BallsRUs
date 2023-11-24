using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Purchase
{
    public class ConnexionPurchaseVM
    {
        public string? NomUtilisateur { get; set; }

        [DataType(DataType.Password)]
        public string? MotDePasse { get; set; }

        public bool estConnecte { get; set; }
    }
}
