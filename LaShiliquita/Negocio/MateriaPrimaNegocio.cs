using System;
using System.Collections.Generic;
using LaShiliquita.Datos;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Negocio
{
    public class MateriaPrimaNegocio
    {
        // Unidades que ofrece la pantalla al crear una materia prima
        public static readonly string[] Unidades = { "kg", "g", "L", "ml", "unidad" };

        // Tipos de movimiento de la BD (para el filtro del historial)
        public static readonly string[] TiposMovimiento = { "SALDO_INICIAL", "COMPRA", "INGRESO", "SALIDA", "AJUSTE", "CONSUMO_PRODUCCION" };

        // Tope por operacion, para evitar errores de digitacion (la columna admite hasta 99,999,999.99)
        public const decimal CantidadMaxima = 1000000m;

        private MateriaPrimaDatos datos = new MateriaPrimaDatos();

        // El modulo "Materia prima y produccion" ya esta limitado por rol en Sesion.TieneAcceso
        // (Gerente, Administrador y Maestro Chocolatero); quien entra puede consultar y operar.
        public bool TieneAcceso()
        {
            return Sesion.TieneAcceso("MateriaPrima");
        }

        // ---------- Consultas ----------

        public List<MateriaPrima> ListarConStock()
        {
            if (!TieneAcceso())
            {
                return new List<MateriaPrima>();
            }
            return datos.ListarConStock();
        }

        // Devuelve "" si el rango es valido; si no, el mensaje del problema
        public string ValidarRango(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
            {
                return "La fecha inicial no puede ser mayor que la fecha final.";
            }
            return "";
        }

        // idMateria = 0 para todas; tipo = "" para todos los tipos
        public List<MovimientoMateriaPrima> ListarMovimientos(DateTime desde, DateTime hasta, int idMateria, string tipo)
        {
            if (!TieneAcceso() || ValidarRango(desde, hasta) != "")
            {
                return new List<MovimientoMateriaPrima>();
            }
            return datos.ListarMovimientos(desde, hasta, idMateria, tipo == null ? "" : tipo);
        }

        // ---------- Alta y edicion de materias primas ----------

        // Valida y guarda una materia prima (nueva si IdMateriaPrima es 0, existente si no).
        // Devuelve "" si todo salio bien (y en una nueva deja el id en materia.IdMateriaPrima);
        // si no, el mensaje del problema. Un fallo de la BD lanza SqlException.
        public string GuardarMateriaPrima(MateriaPrima materia)
        {
            if (!TieneAcceso())
            {
                return "Tu rol no tiene permiso para modificar materias primas.";
            }

            materia.Nombre = (materia.Nombre == null) ? "" : materia.Nombre.Trim();
            materia.UnidadMedida = (materia.UnidadMedida == null) ? "" : materia.UnidadMedida.Trim();
            materia.Descripcion = (materia.Descripcion == null) ? "" : materia.Descripcion.Trim();

            if (materia.Nombre == "")
            {
                return "El nombre es obligatorio.";
            }

            if (materia.Nombre.Length > 100)
            {
                return "El nombre es demasiado largo (máximo 100 caracteres).";
            }

            if (materia.UnidadMedida == "")
            {
                return "Elige la unidad de medida.";
            }

            if (materia.UnidadMedida.Length > 50)
            {
                return "La unidad de medida es demasiado larga (máximo 50 caracteres).";
            }

            if (materia.Descripcion.Length > 200)
            {
                return "La descripción es demasiado larga (máximo 200 caracteres).";
            }

            if (materia.TieneMinimo)
            {
                if (materia.StockMinimo < 0)
                {
                    return "El stock mínimo no puede ser negativo.";
                }

                if (Math.Round(materia.StockMinimo, 2) != materia.StockMinimo)
                {
                    return "El stock mínimo admite máximo 2 decimales.";
                }

                if (materia.StockMinimo > CantidadMaxima)
                {
                    return "El stock mínimo es demasiado grande.";
                }
            }

            // La base no impide nombres repetidos, por eso se revisa aqui
            if (datos.ExisteNombre(materia.Nombre, materia.IdMateriaPrima))
            {
                return "Ya existe una materia prima con ese nombre.";
            }

            if (materia.IdMateriaPrima == 0)
            {
                materia.IdMateriaPrima = datos.Insertar(materia);
            }
            else
            {
                datos.Actualizar(materia);
            }

            return "";
        }

        // ---------- Operaciones de stock: devuelven "" si todo salio bien o el mensaje del problema ----------
        // Un fallo de la BD lanza SqlException: la pantalla debe capturarlo.

        // El motivo es opcional (si va vacio se guarda "Ingreso manual"). Para cargar el stock inicial
        // se escribe "Stock inicial" como motivo.
        public string RegistrarIngreso(int idMateria, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idMateria, cantidad, false);
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
                datos.RegistrarIngreso(idMateria, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // El motivo es obligatorio. Exige stock suficiente.
        public string RegistrarSalida(int idMateria, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idMateria, cantidad, false);
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
                datos.RegistrarSalida(idMateria, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // Cantidad con signo (no puede ser cero). El motivo es obligatorio.
        // Positivo suma; negativo resta y exige stock suficiente.
        public string RegistrarAjuste(int idMateria, decimal cantidad, string motivo)
        {
            string error = ValidarOperacion(idMateria, cantidad, true);
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
                datos.RegistrarAjuste(idMateria, cantidad, motivo, Sesion.UsuarioActual.IdUsuario);
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }

            return "";
        }

        // ---------- Validaciones comunes ----------

        private string ValidarOperacion(int idMateria, decimal cantidad, bool permiteNegativa)
        {
            if (Sesion.UsuarioActual == null)
            {
                return "No hay una sesión activa.";
            }

            if (!TieneAcceso())
            {
                return "Tu rol no tiene permiso para modificar el stock de materia prima.";
            }

            if (idMateria <= 0)
            {
                return "Elige una materia prima.";
            }

            if (cantidad == 0)
            {
                return "La cantidad no puede ser cero.";
            }

            if (cantidad < 0 && !permiteNegativa)
            {
                return "La cantidad debe ser mayor que cero.";
            }

            if (Math.Round(cantidad, 2) != cantidad)
            {
                return "La cantidad admite máximo 2 decimales.";
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