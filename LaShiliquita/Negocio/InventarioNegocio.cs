using System;
using System.Collections.Generic;
using LaShiliquita.Datos;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Negocio
{
    public class InventarioNegocio
    {
        // Tipos de movimiento de la BD (para el filtro del historial)
        public static readonly string[] TiposMovimiento = { "SALDO_INICIAL", "INGRESO", "SALIDA", "AJUSTE", "VENTA", "PRODUCCION" };

        // Tope por operacion, para evitar errores de digitacion
        public const decimal CantidadMaxima = 100000m;

        private InventarioDatos datos = new InventarioDatos();

        // ---------- Permisos dentro del modulo ----------

        // Todos los roles consultan; solo estos tres pueden hacer ingresos, salidas y ajustes
        public bool PuedeOperar()
        {
            if (Sesion.UsuarioActual == null)
            {
                return false;
            }

            string rol = Sesion.UsuarioActual.NombreRol;
            return rol == "Gerente" || rol == "Administrador" || rol == "Maestro Chocolatero";
        }

        // Gerente y Administrador ven (y operan) ambas sedes; los demas, solo la de su sesion
        public bool PuedeElegirSede()
        {
            if (Sesion.UsuarioActual == null)
            {
                return false;
            }

            string rol = Sesion.UsuarioActual.NombreRol;
            return rol == "Gerente" || rol == "Administrador";
        }

        public List<Sede> ObtenerSedesDisponibles()
        {
            if (Sesion.UsuarioActual == null || Sesion.SedeActual == null)
            {
                return new List<Sede>();
            }

            if (PuedeElegirSede())
            {
                return new UsuarioNegocio().ObtenerSedes();
            }

            List<Sede> lista = new List<Sede>();
            lista.Add(Sesion.SedeActual);
            return lista;
        }

        public bool SedePermitida(int idPunto)
        {
            foreach (Sede sede in ObtenerSedesDisponibles())
            {
                if (sede.IdPunto == idPunto)
                {
                    return true;
                }
            }
            return false;
        }

        // ---------- Consultas ----------

        // Devuelve "" si el rango es valido; si no, el mensaje del problema
        public string ValidarRango(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
            {
                return "La fecha inicial no puede ser mayor que la fecha final.";
            }
            return "";
        }

        // Productos con su stock actual en la sede (para las listas de la pantalla)
        public List<ProductoVenta> ListarProductos(int idPunto)
        {
            if (!SedePermitida(idPunto))
            {
                return new List<ProductoVenta>();
            }
            return new VentaDatos().ListarCatalogo(idPunto);
        }

        public List<ResumenInventario> ObtenerResumen(int idPunto, DateTime desde, DateTime hasta)
        {
            if (!SedePermitida(idPunto) || ValidarRango(desde, hasta) != "")
            {
                return new List<ResumenInventario>();
            }
            return datos.ObtenerResumen(idPunto, desde, hasta);
        }

        // idProducto = 0 para todos los productos; tipo = "" para todos los tipos
        public List<MovimientoInventario> ListarMovimientos(int idPunto, DateTime desde, DateTime hasta, int idProducto, string tipo)
        {
            if (!SedePermitida(idPunto) || ValidarRango(desde, hasta) != "")
            {
                return new List<MovimientoInventario>();
            }
            return datos.ListarMovimientos(idPunto, desde, hasta, idProducto, tipo == null ? "" : tipo);
        }

        public List<LoteStock> ListarLotes(int idProducto, int idPunto)
        {
            if (!SedePermitida(idPunto))
            {
                return new List<LoteStock>();
            }
            return datos.ListarLotes(idProducto, idPunto);
        }

        // ---------- Operaciones: devuelven "" si todo salio bien o el mensaje del problema ----------
        // Un fallo de la BD lanza SqlException: la pantalla debe capturarlo.

        // idLote <= 0 significa "sin lote". El motivo es opcional (si va vacio se guarda "Ingreso manual").
        public string RegistrarIngreso(int idPunto, int idProducto, int idLote, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idPunto, idProducto, cantidad, false);
            if (error != "")
            {
                return error;
            }

            motivo = (motivo == null) ? "" : motivo.Trim();
            if (motivo == "")
            {
                motivo = "Ingreso manual";
            }
            if (motivo.Length > 200)
            {
                return "El motivo es demasiado largo (máximo 200 caracteres).";
            }

            try
            {
                datos.RegistrarIngreso(idPunto, idProducto, idLote, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // El motivo es obligatorio. Resta por FEFO.
        public string RegistrarSalida(int idPunto, int idProducto, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idPunto, idProducto, cantidad, false);
            if (error != "")
            {
                return error;
            }

            error = ValidarMotivoObligatorio(ref motivo);
            if (error != "")
            {
                return error;
            }

            try
            {
                datos.RegistrarSalida(idPunto, idProducto, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // Cantidad con signo (no puede ser cero). El motivo es obligatorio.
        // Positivo: suma (en el lote indicado o sin lote). Negativo: resta por FEFO.
        public string RegistrarAjuste(int idPunto, int idProducto, int idLote, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idPunto, idProducto, cantidad, true);
            if (error != "")
            {
                return error;
            }

            error = ValidarMotivoObligatorio(ref motivo);
            if (error != "")
            {
                return error;
            }

            try
            {
                datos.RegistrarAjuste(idPunto, idProducto, idLote, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // ---------- Validaciones comunes ----------

        private string ValidarOperacion(int idPunto, int idProducto, decimal cantidad, bool permiteNegativa)
        {
            if (Sesion.UsuarioActual == null || Sesion.SedeActual == null)
            {
                return "No hay una sesión activa.";
            }

            if (!PuedeOperar())
            {
                return "Tu rol no tiene permiso para modificar el inventario.";
            }

            if (!SedePermitida(idPunto))
            {
                return "No tienes acceso a esa sede.";
            }

            if (idProducto <= 0)
            {
                return "Elige un producto.";
            }

            if (cantidad == 0)
            {
                return "La cantidad no puede ser cero.";
            }

            if (cantidad < 0 && !permiteNegativa)
            {
                return "La cantidad debe ser mayor que cero.";
            }

            if (cantidad != Math.Truncate(cantidad))
            {
                return "La cantidad debe ser un número entero de unidades.";
            }

            if (Math.Abs(cantidad) > CantidadMaxima)
            {
                return "La cantidad es demasiado grande (máximo " + CantidadMaxima.ToString("0") + " por operación).";
            }

            return "";
        }

        // Limpia el motivo y comprueba que no este vacio ni sea demasiado largo
        private string ValidarMotivoObligatorio(ref string motivo)
        {
            motivo = (motivo == null) ? "" : motivo.Trim();

            if (motivo == "")
            {
                return "Escribe el motivo de la operación.";
            }

            if (motivo.Length > 200)
            {
                return "El motivo es demasiado largo (máximo 200 caracteres).";
            }

            return "";
        }
    }
}