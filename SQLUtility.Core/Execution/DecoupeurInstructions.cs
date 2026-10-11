using System.Collections.Generic;
using SQLUtility.Core.Edition;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Découpe un script en instructions sur les « ; » hors chaînes et commentaires.
    /// L'éditeur n'exécute qu'une instruction à la fois (EF-22) : un script en contenant
    /// plusieurs doit être sélectionné instruction par instruction.
    /// </summary>
    public static class DecoupeurInstructions
    {
        /// <summary>
        /// Retourne les instructions non vides (sans le « ; » final). Une portion ne contenant
        /// que des espaces ou des commentaires n'est pas une instruction.
        /// </summary>
        public static IReadOnlyList<string> Decouper(string sql)
        {
            List<string> instructions = new List<string>();
            if (string.IsNullOrEmpty(sql)) return instructions;

            int debut = 0;
            bool significative = false;
            foreach (JetonSql jeton in TokeniseurSql.Tokeniser(sql))
            {
                if (jeton.Type == TypeJetonSql.Ponctuation && jeton.Texte == ";")
                {
                    if (significative) instructions.Add(sql.Substring(debut, jeton.Position - debut).Trim());
                    debut = jeton.Position + 1;
                    significative = false;
                    continue;
                }
                if (jeton.Type != TypeJetonSql.Espace && jeton.Type != TypeJetonSql.Commentaire)
                    significative = true;
            }
            if (significative) instructions.Add(sql.Substring(debut).Trim());
            return instructions;
        }
    }
}
