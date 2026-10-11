using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Execution
{
    /// <summary>Résultat de l'analyse de risque d'une instruction SQL. (EF-22 à EF-32)</summary>
    public sealed class EvaluationRisque
    {
        public EvaluationRisque(TypeInstructionSql type, NiveauRisque niveau,
                                IList<string> raisons, bool confirmationRenforcee)
        {
            Type = type;
            Niveau = niveau;
            Raisons = new ReadOnlyCollection<string>(raisons ?? new List<string>());
            ConfirmationRenforcee = confirmationRenforcee;
        }

        public TypeInstructionSql Type { get; }
        public NiveauRisque Niveau { get; }

        /// <summary>Facteurs de risque à présenter à l'utilisateur.</summary>
        public IReadOnlyList<string> Raisons { get; }

        /// <summary>Vrai si une confirmation renforcée (ex. ressaisie) est requise.</summary>
        public bool ConfirmationRenforcee { get; }

        /// <summary>Vrai si l'instruction ne modifie rien (SELECT) : exécutable sans confirmation.</summary>
        public bool EstLectureSeule
        {
            get { return Type == TypeInstructionSql.Select; }
        }
    }
}
