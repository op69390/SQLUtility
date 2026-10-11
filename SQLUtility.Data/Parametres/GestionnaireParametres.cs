using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SQLUtility.Data.Parametres
{
    /// <summary>
    /// Charge et enregistre les <see cref="ParametresApplication"/> au format JSON,
    /// sous %APPDATA%\SQLUtility\parametres.json. L'écriture est atomique
    /// (fichier temporaire puis remplacement) pour éviter toute corruption.
    /// </summary>
    public sealed class GestionnaireParametres
    {
        private static readonly JsonSerializerSettings Reglages = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        // Encodage UTF-8 sans BOM pour le fichier de données JSON.
        private static readonly UTF8Encoding EncodageJson = new UTF8Encoding(false);

        public GestionnaireParametres()
        {
            string baseAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            RepertoireApplication = Path.Combine(baseAppData, "SQLUtility");
            CheminFichier = Path.Combine(RepertoireApplication, "parametres.json");
        }

        /// <summary>Surcharge l'emplacement du fichier (tests, profils multiples).</summary>
        public GestionnaireParametres(string cheminFichier)
        {
            if (string.IsNullOrEmpty(cheminFichier)) throw new ArgumentNullException(nameof(cheminFichier));
            CheminFichier = cheminFichier;
            RepertoireApplication = Path.GetDirectoryName(cheminFichier);
        }

        /// <summary>Répertoire contenant le fichier de paramètres.</summary>
        public string RepertoireApplication { get; }

        /// <summary>Chemin complet du fichier de paramètres.</summary>
        public string CheminFichier { get; }

        /// <summary>
        /// Charge les paramètres. Si le fichier est absent, crée des paramètres par
        /// défaut et les enregistre (comportement de premier démarrage).
        /// </summary>
        public ParametresApplication Charger()
        {
            if (!File.Exists(CheminFichier))
            {
                ParametresApplication defaut = new ParametresApplication();
                Enregistrer(defaut);
                return defaut;
            }

            string json = File.ReadAllText(CheminFichier, EncodageJson);
            ParametresApplication parametres = JsonConvert.DeserializeObject<ParametresApplication>(json, Reglages);
            return parametres ?? new ParametresApplication();
        }

        /// <summary>Enregistre les paramètres (écriture atomique via fichier temporaire).</summary>
        public void Enregistrer(ParametresApplication parametres)
        {
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));

            if (!string.IsNullOrEmpty(RepertoireApplication) && !Directory.Exists(RepertoireApplication))
                Directory.CreateDirectory(RepertoireApplication);

            string json = JsonConvert.SerializeObject(parametres, Reglages);
            string cheminTemp = CheminFichier + ".tmp";

            File.WriteAllText(cheminTemp, json, EncodageJson);

            if (File.Exists(CheminFichier))
                File.Replace(cheminTemp, CheminFichier, null);
            else
                File.Move(cheminTemp, CheminFichier);
        }
    }
}
