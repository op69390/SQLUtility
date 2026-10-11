using System.Collections.Generic;
using SQLUtility.Core.Connexions;

namespace SQLUtility.Data.Parametres
{
    /// <summary>Paramètres applicatifs persistés (profils de connexion, préférences).</summary>
    public sealed class ParametresApplication
    {
        /// <summary>Version du schéma de paramètres (pour migrations futures).</summary>
        public int Version { get; set; }

        /// <summary>Profils de connexion enregistrés. (EF-03)</summary>
        public List<ConnexionProfil> Profils { get; set; }

        /// <summary>Nom du profil sélectionné par défaut.</summary>
        public string ProfilParDefaut { get; set; }

        /// <summary>Dernier répertoire utilisé pour l'enregistrement de fichiers .sql. (EF-33)</summary>
        public string DernierRepertoireSql { get; set; }

        public ParametresApplication()
        {
            Version = 1;
            Profils = new List<ConnexionProfil>();
        }
    }
}
