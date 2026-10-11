using System;
using System.Data.Odbc;
using System.Diagnostics;
using SQLUtility.Core.Execution;

namespace SQLUtility.Data.Providers
{
    /// <summary>
    /// DML exécuté via ODBC (EF-22 à EF-32). Sous contrôle de validation, la connexion et la
    /// transaction restent ouvertes — et les lignes modifiées verrouillées — jusqu'à
    /// <see cref="Valider"/> ou <see cref="Annuler"/>. Sans décision, <see cref="Dispose"/> annule.
    /// </summary>
    internal sealed class TransactionExecutionOdbc : ITransactionExecution
    {
        private OdbcConnection pConnexion;
        private OdbcTransaction pTransaction;

        /// <summary>Opération sous contrôle de validation : la transaction reste à valider ou annuler.</summary>
        public TransactionExecutionOdbc(OdbcConnection connexion, OdbcTransaction transaction, int lignesAffectees)
        {
            if (connexion == null) throw new ArgumentNullException(nameof(connexion));
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            pConnexion = connexion;
            pTransaction = transaction;
            LignesAffectees = lignesAffectees;
        }

        /// <summary>Opération déjà validée (validation immédiate) : rien à piloter.</summary>
        public TransactionExecutionOdbc(int lignesAffectees)
        {
            LignesAffectees = lignesAffectees;
        }

        public int LignesAffectees { get; }

        public bool SousControleValidation
        {
            get { return pTransaction != null; }
        }

        public void Valider()
        {
            if (pTransaction == null) return;
            pTransaction.Commit();
            Fermer();
        }

        public void Annuler()
        {
            if (pTransaction == null) return;
            pTransaction.Rollback();
            Fermer();
        }

        public void Dispose()
        {
            if (pTransaction != null)
            {
                // Sortie sans décision explicite : on annule, choix sûr (EF-22).
                try
                {
                    pTransaction.Rollback();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("TransactionExecutionOdbc : ROLLBACK implicite en échec : " + ex.Message);
                }
            }
            Fermer();
        }

        private void Fermer()
        {
            if (pTransaction != null)
            {
                pTransaction.Dispose();
                pTransaction = null;
            }
            if (pConnexion != null)
            {
                pConnexion.Dispose();
                pConnexion = null;
            }
        }
    }
}
