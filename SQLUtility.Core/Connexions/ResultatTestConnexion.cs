using System;

namespace SQLUtility.Core.Connexions
{
    /// <summary>Résultat d'un test de connexion. (EF-04)</summary>
    public sealed class ResultatTestConnexion
    {
        public ResultatTestConnexion(bool succes, string message, TimeSpan duree)
        {
            Succes = succes;
            Message = message;
            Duree = duree;
        }

        /// <summary>Vrai si la connexion a pu être ouverte.</summary>
        public bool Succes { get; }

        /// <summary>Message à présenter à l'utilisateur (succès ou cause de l'échec).</summary>
        public string Message { get; }

        /// <summary>Durée mesurée de l'ouverture.</summary>
        public TimeSpan Duree { get; }

        public static ResultatTestConnexion Reussi(TimeSpan duree)
        {
            return new ResultatTestConnexion(true, "Connexion réussie.", duree);
        }

        public static ResultatTestConnexion Echec(string message)
        {
            return new ResultatTestConnexion(false, message, TimeSpan.Zero);
        }
    }
}
