using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Account
{
    public class LogInVM
    {
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; }

        [Display(Name = "Mot de Passe")]
        [DataType(DataType.Password)]
        public string MotDePasse { get; set; }
    }
}
