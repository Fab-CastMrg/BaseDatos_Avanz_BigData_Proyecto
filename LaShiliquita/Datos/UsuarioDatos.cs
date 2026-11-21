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
    }
}