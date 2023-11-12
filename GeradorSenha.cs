using System.Security.Cryptography;

namespace SetupDatabase
{
    internal class GeradorSenha
    {
        public string GerarSenha(int comprimento)
        {
            const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$&";
            char[] senha = new char[comprimento];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[comprimento * 4];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < comprimento; i++)
                {
                    int valorInteiro = BitConverter.ToInt32(randomBytes, i * 4);
                    int indiceCaracter = Math.Abs(valorInteiro % caracteresPermitidos.Length);
                    senha[i] = caracteresPermitidos[indiceCaracter];
                }
            }

            return new string(senha);
        }

    }
}
