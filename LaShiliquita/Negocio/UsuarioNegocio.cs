using System.Collections.Generic;
using LaShiliquita.Datos;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Negocio
{
    public class UsuarioNegocio
    {
        private UsuarioDatos datos = new UsuarioDatos();

        // Devuelve el usuario si el nombre y la contrasena son correctos y esta activo; si no, null.
        // No se indica cual de los datos fallo, para no dar pistas.
        public Usuario Autenticar(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(contrasena))
            {
                return null;
            }

            Usuario usuario = datos.BuscarPorNombreUsuario(nombreUsuario.Trim());

            if (usuario == null || !usuario.Activo)
            {
                return null;
            }

            if (!Seguridad.Verificar(contrasena, usuario.Contrasena))
            {
                return null;
            }

            usuario.Contrasena = "";   // el hash no se queda en memoria durante la sesion
            return usuario;
        }

        public List<Sede> ObtenerSedes()
        {
            return datos.ObtenerSedesConCaja();
        }

        // Convierte a hash las contrasenas que estan en texto plano. Devuelve cuantas convirtio.
        // Se puede ejecutar varias veces: solo toca las que aun no tienen hash.
        public int MigrarContrasenas()
        {
            List<Usuario> pendientes = datos.ObtenerUsuariosSinHash();

            foreach (Usuario usuario in pendientes)
            {
                string hash = Seguridad.CrearHash(usuario.Contrasena);
                datos.ActualizarContrasena(usuario.IdUsuario, hash);
            }

            return pendientes.Count;
        }
    }
}