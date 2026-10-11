namespace SQLUtility.Core.Connexions
{
    /// <summary>
    /// Profil de connexion à une base (EF-03 à EF-06).
    /// Le mot de passe n'est jamais stocké en clair : seul <see cref="SecretChiffre"/>
    /// (chiffré DPAPI, encodé Base64) est conservé.
    /// </summary>
    public sealed class ConnexionProfil
    {
        /// <summary>Nom convivial du profil (identifiant d'affichage).</summary>
        public string Nom { get; set; }

        /// <summary>Type de SGBD ciblé.</summary>
        public TypeSgbd Type { get; set; }

        /// <summary>Nom système ou adresse IP de la partition IBM i.</summary>
        public string Serveur { get; set; }

        /// <summary>Profil utilisateur de connexion.</summary>
        public string Utilisateur { get; set; }

        /// <summary>Secret (mot de passe) chiffré DPAPI, encodé Base64. Jamais en clair. (EF-05)</summary>
        public string SecretChiffre { get; set; }

        /// <summary>Bibliothèque / liste de bibliothèques par défaut.</summary>
        public string BibliothequeParDefaut { get; set; }

        /// <summary>Connexion en lecture seule : interdit tout DML / DDL. (EF-06)</summary>
        public bool LectureSeule { get; set; }

        /// <summary>Commentaire libre.</summary>
        public string Description { get; set; }

        public ConnexionProfil()
        {
            Type = TypeSgbd.Db2ForI;
            LectureSeule = true; // Sécurité par défaut (EF-06).
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Nom) ? "(profil sans nom)" : Nom;
        }
    }
}
