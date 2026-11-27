using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Datos
{
    public class UsuarioDatos
    {
        // Busca un usuario por su nombre_usuario junto con el nombre de su rol. Devuelve null si no existe.
        public Usuario BuscarPorNombreUsuario(string nombreUsuario)
        {
            Usuario usuario = null;

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT u.id_usuario, u.id_rol, r.nombre_rol, u.nombres, u.apellidos, " +
                             "u.nombre_usuario, u.contrasena, u.activo " +
                             "FROM usuarios u INNER JOIN roles r ON r.id_rol = u.id_rol " +
                             "WHERE u.nombre_usuario = @nombreUsuario";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        usuario = new Usuario();
                        usuario.IdUsuario = Convert.ToInt32(dr["id_usuario"]);
                        usuario.IdRol = Convert.ToInt32(dr["id_rol"]);
                        usuario.NombreRol = dr["nombre_rol"].ToString();
                        usuario.Nombres = dr["nombres"].ToString();
                        usuario.Apellidos = dr["apellidos"].ToString();
                        usuario.NombreUsuario = dr["nombre_usuario"].ToString();
                        usuario.Contrasena = dr["contrasena"].ToString();
                        usuario.Activo = Convert.ToInt32(dr["activo"]) == 1;
                    }
                }
            }

            return usuario;
        }

        // Puntos de venta que tienen caja (Tienda Principal y Hacienda La Colpa)
        public List<Sede> ObtenerSedesConCaja()
        {
            List<Sede> lista = new List<Sede>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT p.id_punto, p.nombre, c.id_caja " +
                             "FROM puntos_abastecimiento p INNER JOIN cajas c ON c.id_punto = p.id_punto " +
                             "ORDER BY p.id_punto";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Sede sede = new Sede();
                        sede.IdPunto = Convert.ToInt32(dr["id_punto"]);
                        sede.Nombre = dr["nombre"].ToString();
                        sede.IdCaja = Convert.ToInt32(dr["id_caja"]);
                        lista.Add(sede);
                    }
                }
            }

            return lista;
        }

        // Usuarios cuya contrasena todavia esta en texto plano (no tiene el prefijo del hash)
        public List<Usuario> ObtenerUsuariosSinHash()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT id_usuario, contrasena FROM usuarios WHERE contrasena NOT LIKE @prefijo";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@prefijo", Seguridad.Prefijo + "$%");
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Usuario usuario = new Usuario();
                        usuario.IdUsuario = Convert.ToInt32(dr["id_usuario"]);
                        usuario.Contrasena = dr["contrasena"].ToString();
                        lista.Add(usuario);
                    }
                }
            }

            return lista;
        }

        public void ActualizarContrasena(int idUsuario, string contrasenaHash)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "UPDATE usuarios SET contrasena = @contrasena WHERE id_usuario = @id";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@contrasena", contrasenaHash);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Todos los usuarios con el nombre de su rol (LEFT JOIN para no ocultar a nadie)
        public List<Usuario> ListarUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT u.id_usuario, u.id_rol, r.nombre_rol, u.nombres, u.apellidos, " +
                             "u.dni, u.telefono, u.nombre_usuario, u.activo " +
                             "FROM usuarios u LEFT JOIN roles r ON r.id_rol = u.id_rol " +
                             "ORDER BY u.nombres, u.apellidos";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Usuario usuario = new Usuario();
                        usuario.IdUsuario = Convert.ToInt32(dr["id_usuario"]);
                        usuario.IdRol = dr["id_rol"] == DBNull.Value ? 0 : Convert.ToInt32(dr["id_rol"]);
                        usuario.NombreRol = dr["nombre_rol"].ToString();
                        usuario.Nombres = dr["nombres"].ToString();
                        usuario.Apellidos = dr["apellidos"].ToString();
                        usuario.Dni = dr["dni"].ToString();
                        usuario.Telefono = dr["telefono"].ToString();
                        usuario.NombreUsuario = dr["nombre_usuario"].ToString();
                        usuario.Activo = Convert.ToInt32(dr["activo"]) == 1;
                        lista.Add(usuario);
                    }
                }
            }

            return lista;
        }

        public List<Rol> ListarRoles()
        {
            List<Rol> lista = new List<Rol>();

            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT id_rol, nombre_rol FROM roles ORDER BY id_rol", cn);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Rol rol = new Rol();
                        rol.IdRol = Convert.ToInt32(dr["id_rol"]);
                        rol.NombreRol = dr["nombre_rol"].ToString();
                        lista.Add(rol);
                    }
                }
            }

            return lista;
        }

        // Indica si otro usuario (distinto de idExcluir) ya usa ese nombre_usuario. Para un usuario nuevo se envia 0.
        public bool ExisteNombreUsuario(string nombreUsuario, int idExcluir)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "SELECT COUNT(*) FROM usuarios WHERE nombre_usuario = @nombreUsuario AND id_usuario <> @idExcluir";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@nombreUsuario", nombreUsuario);
                cmd.Parameters.AddWithValue("@idExcluir", idExcluir);
                cn.Open();

                int cantidad = (int)cmd.ExecuteScalar();
                return cantidad > 0;
            }
        }

        // Inserta un usuario nuevo (activo) y devuelve su id
        public int InsertarUsuario(Usuario usuario, string contrasenaHash)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "INSERT INTO usuarios (id_rol, nombres, apellidos, dni, telefono, nombre_usuario, contrasena, activo) " +
                             "VALUES (@idRol, @nombres, @apellidos, @dni, @telefono, @nombreUsuario, @contrasena, 1); " +
                             "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@idRol", usuario.IdRol);
                cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
                cmd.Parameters.AddWithValue("@apellidos", usuario.Apellidos);
                cmd.Parameters.AddWithValue("@dni", TextoONulo(usuario.Dni));
                cmd.Parameters.AddWithValue("@telefono", TextoONulo(usuario.Telefono));
                cmd.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@contrasena", contrasenaHash);
                cn.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        // Actualiza los datos de un usuario. Si contrasenaHash viene vacio, la contrasena no se toca.
        public void ActualizarUsuario(Usuario usuario, string contrasenaHash)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "UPDATE usuarios SET id_rol = @idRol, nombres = @nombres, apellidos = @apellidos, " +
                             "dni = @dni, telefono = @telefono, nombre_usuario = @nombreUsuario";

                if (contrasenaHash != "")
                {
                    sql += ", contrasena = @contrasena";
                }
                sql += " WHERE id_usuario = @id";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@idRol", usuario.IdRol);
                cmd.Parameters.AddWithValue("@nombres", usuario.Nombres);
                cmd.Parameters.AddWithValue("@apellidos", usuario.Apellidos);
                cmd.Parameters.AddWithValue("@dni", TextoONulo(usuario.Dni));
                cmd.Parameters.AddWithValue("@telefono", TextoONulo(usuario.Telefono));
                cmd.Parameters.AddWithValue("@nombreUsuario", usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);

                if (contrasenaHash != "")
                {
                    cmd.Parameters.AddWithValue("@contrasena", contrasenaHash);
                }

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void CambiarEstado(int idUsuario, bool activo)
        {
            using (SqlConnection cn = Conexion.ObtenerConexion())
            {
                string sql = "UPDATE usuarios SET activo = @activo WHERE id_usuario = @id";

                SqlCommand cmd = new SqlCommand(sql, cn);
                cmd.Parameters.AddWithValue("@activo", activo ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Los campos opcionales vacios se guardan como NULL
        private object TextoONulo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return DBNull.Value;
            }
            return texto.Trim();
        }
    }
}