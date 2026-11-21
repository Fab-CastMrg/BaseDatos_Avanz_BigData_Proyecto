using System;
using System.Security.Cryptography;

namespace LaShiliquita.Utilidades
{
    // Hash de contrasenas con PBKDF2 (SHA-256). Requiere .NET Framework 4.7.2 o superior.
    // Formato guardado en la BD: PBKDF2$iteraciones$sal$hash  (unos 81 caracteres, caben en VARCHAR(100))
    public class Seguridad
    {
        public const string Prefijo = "PBKDF2";
        private const int Iteraciones = 10000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        // Convierte una contrasena en el texto que se guarda en la BD
        public static string CrearHash(string contrasena)
        {
            byte[] sal = new byte[TamanoSal];
            using (RandomNumberGenerator generador = RandomNumberGenerator.Create())
            {
                generador.GetBytes(sal);
            }
            byte[] hash = CalcularHash(contrasena, sal, Iteraciones);
            return Prefijo + "$" + Iteraciones + "$" + Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        // Indica si un texto guardado ya tiene formato de hash
        public static bool EsHash(string guardado)
        {
            return guardado != null && guardado.StartsWith(Prefijo + "$");
        }

        // Compara la contrasena escrita con el hash guardado
        public static bool Verificar(string contrasena, string guardado)
        {
            if (!EsHash(guardado))
            {
                return false;
            }

            string[] partes = guardado.Split('$');
            if (partes.Length != 4)
            {
                return false;
            }

            int iteraciones;
            if (!int.TryParse(partes[1], out iteraciones))
            {
                return false;
            }

            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] hashGuardado = Convert.FromBase64String(partes[3]);
            byte[] hashNuevo = CalcularHash(contrasena, sal, iteraciones);

            return SonIguales(hashGuardado, hashNuevo);
        }

        private static byte[] CalcularHash(string contrasena, byte[] sal, int iteraciones)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(contrasena, sal, iteraciones, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(TamanoHash);
        }

        // Compara todos los bytes siempre, para no dar pistas por el tiempo de respuesta
        private static bool SonIguales(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diferencia |= a[i] ^ b[i];
            }
            return diferencia == 0;
        }
    }
}
