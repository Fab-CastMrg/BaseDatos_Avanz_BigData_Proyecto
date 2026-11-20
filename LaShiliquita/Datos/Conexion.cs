using System.Configuration;
using System.Data.SqlClient;

namespace LaShiliquita.Datos
{
    public class Conexion
    {
        // Lee la cadena de conexion desde App.config
        public static string CadenaConexion
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ShiliquitaDB"].ConnectionString;
            }
        }

        // Devuelve una conexion nueva (sin abrir). Quien la use debe abrirla y cerrarla.
        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(CadenaConexion);
        }

        // Prueba la conexion y devuelve un mensaje con el resultado
        public static string ProbarConexion()
        {
            try
            {
                using (SqlConnection cn = ObtenerConexion())
                {
                    cn.Open();
                    return "Conexion exitosa a la base " + cn.Database;
                }
            }
            catch (SqlException ex)
            {
                return "No se pudo conectar: " + ex.Message;
            }
        }
    }
}