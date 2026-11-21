using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LaShiliquita.Entidades;

namespace LaShiliquita.Utilidades
{
    // Guarda el usuario y la sede de la sesion actual, y decide a que modulos tiene acceso cada rol
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }
        public static Sede SedeActual { get; set; }

        public static void Cerrar()
        {
            UsuarioActual = null;
            SedeActual = null;
        }

        // Permisos por modulo. Los nombres de rol son los de la tabla roles.
        // Para cambiar quien ve cada modulo, se edita solo este metodo.
        public static bool TieneAcceso(string modulo)
        {
            if (UsuarioActual == null)
            {
                return false;
            }

            string rol = UsuarioActual.NombreRol;

            switch (modulo)
            {
                case "Dashboard":
                case "Ventas":
                case "Inventario":
                    return true;
                case "MateriaPrima":
                    return rol == "Gerente" || rol == "Administrador" || rol == "Maestro Chocolatero";
                case "Caja":
                    return rol == "Gerente" || rol == "Administrador" || rol == "Vendedor";
                case "Proveedores":
                case "Reportes":
                case "Usuarios":
                    return rol == "Gerente" || rol == "Administrador";
                default:
                    return false;
            }
        }
    }
}
