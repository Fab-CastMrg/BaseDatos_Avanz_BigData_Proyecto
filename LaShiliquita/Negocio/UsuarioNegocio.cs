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

        // ---------- Administracion de usuarios ----------

        public List<Usuario> ListarUsuarios()
        {
            return datos.ListarUsuarios();
        }

        public List<Rol> ListarRoles()
        {
            return datos.ListarRoles();
        }

        // Valida y guarda un usuario (nuevo si IdUsuario es 0, existente si no).
        // Devuelve "" si todo salio bien; si no, el mensaje del problema.
        // contrasena: obligatoria para un usuario nuevo; en uno existente, vacia significa "no cambiar".
        public string GuardarUsuario(Usuario usuario, string contrasena)
        {
            bool esNuevo = usuario.IdUsuario == 0;

            usuario.Nombres = usuario.Nombres.Trim();
            usuario.Apellidos = usuario.Apellidos.Trim();
            usuario.Dni = usuario.Dni.Trim();
            usuario.Telefono = usuario.Telefono.Trim();
            usuario.NombreUsuario = usuario.NombreUsuario.Trim();

            if (usuario.Nombres == "")
            {
                return "Ingrese los nombres.";
            }
            if (usuario.Apellidos == "")
            {
                return "Ingrese los apellidos.";
            }
            if (usuario.NombreUsuario.Length < 4)
            {
                return "El usuario debe tener al menos 4 caracteres.";
            }
            if (usuario.NombreUsuario.Contains(" "))
            {
                return "El usuario no puede tener espacios.";
            }
            if (usuario.IdRol <= 0)
            {
                return "Seleccione un rol.";
            }
            if (usuario.Dni != "" && (usuario.Dni.Length != 8 || !SoloDigitos(usuario.Dni)))
            {
                return "El DNI debe tener 8 dígitos.";
            }
            if (usuario.Telefono != "" && (usuario.Telefono.Length < 6 || usuario.Telefono.Length > 15 || !SoloDigitos(usuario.Telefono)))
            {
                return "El teléfono debe tener solo números (entre 6 y 15 dígitos).";
            }

            if (esNuevo && contrasena == "")
            {
                return "Ingrese una contraseña para el nuevo usuario.";
            }
            if (contrasena != "" && contrasena.Length < 6)
            {
                return "La contraseña debe tener al menos 6 caracteres.";
            }
            if (contrasena.StartsWith(Seguridad.Prefijo + "$"))
            {
                return "Esa contraseña no está permitida.";
            }

            // Nadie puede cambiar su propio rol (evita quedarse sin acceso por error)
            if (!esNuevo && usuario.IdUsuario == Sesion.UsuarioActual.IdUsuario && usuario.IdRol != Sesion.UsuarioActual.IdRol)
            {
                return "No puede cambiar su propio rol.";
            }

            if (datos.ExisteNombreUsuario(usuario.NombreUsuario, usuario.IdUsuario))
            {
                return "Ya existe un usuario con ese nombre de usuario.";
            }

            string hash = "";
            if (contrasena != "")
            {
                hash = Seguridad.CrearHash(contrasena);
            }

            if (esNuevo)
            {
                usuario.IdUsuario = datos.InsertarUsuario(usuario, hash);
            }
            else
            {
                datos.ActualizarUsuario(usuario, hash);
            }

            return "";
        }

        // Activa o desactiva un usuario. Devuelve "" si salio bien; si no, el mensaje del problema.
        public string CambiarEstado(Usuario usuario, bool activar)
        {
            if (!activar && usuario.IdUsuario == Sesion.UsuarioActual.IdUsuario)
            {
                return "No puede desactivar su propio usuario.";
            }

            datos.CambiarEstado(usuario.IdUsuario, activar);
            return "";
        }

        private bool SoloDigitos(string texto)
        {
            foreach (char c in texto)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
        }
    }
}