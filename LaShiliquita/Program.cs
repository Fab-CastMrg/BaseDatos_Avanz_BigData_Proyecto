using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using LaShiliquita.Negocio;
using LaShiliquita.Presentacion;

namespace LaShiliquita
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Convierte a hash las contrasenas que sigan en texto plano.
            // Solo actua la primera vez; despues no encuentra nada que convertir.
            try
            {
                UsuarioNegocio usuarioNegocio = new UsuarioNegocio();
                int migradas = usuarioNegocio.MigrarContrasenas();

                if (migradas > 0)
                {
                    MessageBox.Show("Se protegieron " + migradas + " contraseñas con hash. " +
                        "Los usuarios siguen entrando con las mismas contraseñas de antes.",
                        "Seguridad", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("No se pudo conectar con la base de datos:\r\n" + ex.Message,
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Login -> ventana principal; si el usuario cierra sesion, vuelve al login
            bool volverAlLogin = true;

            while (volverAlLogin)
            {
                FrmLogin login = new FrmLogin();

                if (login.ShowDialog() != DialogResult.OK)
                {
                    break;   // cerro el login sin ingresar
                }

                FrmPrincipal principal = new FrmPrincipal();
                Application.Run(principal);
                volverAlLogin = principal.CerroSesion;
            }
        }
    }
}