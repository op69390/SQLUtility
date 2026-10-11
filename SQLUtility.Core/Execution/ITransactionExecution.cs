using System;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Représente l'exécution d'un DML en attente de validation, pilotable par l'appelant
    /// (COMMIT / ROLLBACK), sans exposer la technologie d'accès. (EF-22 à EF-32)
    /// Si <see cref="SousControleValidation"/> est faux, l'opération est déjà validée
    /// (autocommit, ex. table non journalisée) et Valider/Annuler sont sans effet.
    /// </summary>
    public interface ITransactionExecution : IDisposable
    {
        /// <summary>Nombre de lignes affectées par l'instruction.</summary>
        int LignesAffectees { get; }

        /// <summary>Vrai si l'opération peut encore être validée ou annulée.</summary>
        bool SousControleValidation { get; }

        /// <summary>Valide définitivement l'opération (COMMIT).</summary>
        void Valider();

        /// <summary>Annule l'opération (ROLLBACK).</summary>
        void Annuler();
    }
}
