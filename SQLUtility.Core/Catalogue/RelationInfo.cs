using System;
using System.Collections.Generic;

namespace SQLUtility.Core.Catalogue
{
    /// <summary>
    /// Relation entre une table enfant et une table parente (clé étrangère). (EF-10)
    /// Les colonnes enfant et parent sont appariées par position.
    /// </summary>
    public sealed class RelationInfo
    {
        public RelationInfo()
        {
            ColonnesEnfant = new List<string>();
            ColonnesParent = new List<string>();
        }

        /// <summary>Schéma de la contrainte.</summary>
        public string Schema { get; set; }

        /// <summary>Nom de la contrainte.</summary>
        public string Nom { get; set; }

        public string SchemaEnfant { get; set; }
        public string TableEnfant { get; set; }
        public List<string> ColonnesEnfant { get; private set; }

        public string SchemaParent { get; set; }
        public string TableParent { get; set; }
        public List<string> ColonnesParent { get; private set; }

        /// <summary>Règle de mise à jour (NO ACTION, RESTRICT).</summary>
        public string RegleMiseAJour { get; set; }

        /// <summary>Règle de suppression (NO ACTION, CASCADE, SET NULL, SET DEFAULT, RESTRICT).</summary>
        public string RegleSuppression { get; set; }

        /// <summary>Vrai si la relation est déclarée dans le catalogue ; faux si elle est déduite.</summary>
        public bool EstDeclaree { get; set; }

        /// <summary>Table enfant qualifiée (SCHEMA.TABLE), pour l'affichage.</summary>
        public string Enfant
        {
            get { return SchemaEnfant + "." + TableEnfant; }
        }

        /// <summary>Table parente qualifiée (SCHEMA.TABLE), pour l'affichage.</summary>
        public string Parent
        {
            get { return SchemaParent + "." + TableParent; }
        }

        /// <summary>Correspondance des colonnes (ENF1 = PAR1, ENF2 = PAR2), pour l'affichage.</summary>
        public string Correspondance
        {
            get
            {
                List<string> paires = new List<string>();
                int nombre = Math.Min(ColonnesEnfant.Count, ColonnesParent.Count);
                for (int i = 0; i < nombre; i++)
                    paires.Add(ColonnesEnfant[i] + " = " + ColonnesParent[i]);
                return string.Join(", ", paires);
            }
        }

        public override string ToString()
        {
            return Nom + " : " + Enfant + " → " + Parent;
        }
    }
}
