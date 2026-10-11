using System;
using System.Windows.Forms;
using SQLUtility.Data.Parametres;

namespace SQLUtility.App
{
    /// <summary>Point d'entrée de l'application.</summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Premier démarrage : Charger() crée le fichier de paramètres s'il est absent.
            GestionnaireParametres gestionnaire = new GestionnaireParametres();
            ParametresApplication parametres = gestionnaire.Charger();

            Application.Run(new MainForm(gestionnaire, parametres));
        }
    }
}
