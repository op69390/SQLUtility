using System;
using SQLUtility.Core.Connexions;

namespace SQLUtility.Data.Providers
{
    /// <summary>Fabrique le bon <see cref="IConnexionProvider"/> selon le profil ou le type.</summary>
    public static class ConnexionProviderFactory
    {
        public static IConnexionProvider Creer(ConnexionProfil profil)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            return Creer(profil.Type);
        }

        public static IConnexionProvider Creer(TypeSgbd type)
        {
            switch (type)
            {
                case TypeSgbd.Db2ForI:
                    return new Db2ForIConnexionProvider();
                default:
                    throw new NotSupportedException("Type de SGBD non pris en charge : " + type + ".");
            }
        }
    }
}
