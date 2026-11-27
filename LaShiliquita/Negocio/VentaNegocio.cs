using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using LaShiliquita.Datos;
using LaShiliquita.Entidades;
using LaShiliquita.Utilidades;

namespace LaShiliquita.Negocio
{
    public class VentaNegocio
    {
        // id_tipo_movimiento = 1 es "Venta" en la tabla tipos_movimiento
        public const int IdTipoMovimientoVenta = 1;

        // Reglas del CHECK de ventas: descuento de 0 a 10 soles y solo si el subtotal supera 200
        public const decimal DescuentoMaximo = 10m;
        public const decimal SubtotalMinimoParaDescuento = 200m;

        public static readonly string[] MetodosPago = { "Efectivo", "Yape", "Plin", "Tarjeta" };

        private VentaDatos datos = new VentaDatos();

        // Catalogo con el stock de la sede de la sesion
        public List<ProductoVenta> ListarCatalogo()
        {
            return datos.ListarCatalogo(Sesion.SedeActual.IdPunto);
        }

        // ---------- Calculos ----------

        // Tasa del IGV desde App.config (se lee con cultura invariante porque el equipo usa coma decimal)
        public decimal ObtenerTasaIgv()
        {
            string texto = ConfigurationManager.AppSettings["IgvTasa"];
            decimal tasa;

            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out tasa))
            {
                tasa = 0.18m;
            }

            return tasa;
        }

        public decimal CalcularSubtotal(List<ItemCarrito> items)
        {
            decimal subtotal = 0;

            foreach (ItemCarrito item in items)
            {
                subtotal = subtotal + item.Subtotal;
            }

            return subtotal;
        }

        // Los precios ya incluyen IGV (PreciosIncluyenIgv debe seguir en true en App.config):
        // base = total / (1 + tasa) e IGV = total - base. El IGV no se suma al total guardado.
        public decimal CalcularBaseImponible(decimal total)
        {
            return Math.Round(total / (1 + ObtenerTasaIgv()), 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalcularIgv(decimal total)
        {
            return total - CalcularBaseImponible(total);
        }

        // El descuento solo se permite cuando el subtotal supera 200 soles
        public bool DescuentoPermitido(decimal subtotal)
        {
            return subtotal > SubtotalMinimoParaDescuento;
        }

        // Devuelve "" si el descuento es valido; si no, el mensaje del problema
        public string ValidarDescuento(decimal subtotal, decimal descuento)
        {
            if (descuento < 0)
            {
                return "El descuento no puede ser negativo.";
            }

            if (descuento == 0)
            {
                return "";
            }

            if (!DescuentoPermitido(subtotal))
            {
                return "El descuento solo se permite cuando el subtotal supera S/ " + SubtotalMinimoParaDescuento.ToString("0.00") + ".";
            }

            if (descuento > DescuentoMaximo)
            {
                return "El descuento máximo es de S/ " + DescuentoMaximo.ToString("0.00") + ".";
            }

            return "";
        }

        // ---------- Registrar la venta ----------

        // Valida y guarda la venta. Devuelve "" si todo salio bien (y el numero de ticket en idVenta);
        // si no, el mensaje del problema (idVenta queda en 0). Un fallo de la BD lanza SqlException:
        // la pantalla debe capturarlo.
        public string RegistrarVenta(List<ItemCarrito> items, decimal descuento, List<PagoVenta> pagos, out int idVenta)
        {
            idVenta = 0;

            if (Sesion.UsuarioActual == null || Sesion.SedeActual == null)
            {
                return "No hay una sesión activa.";
            }

            if (items == null || items.Count == 0)
            {
                return "El carrito está vacío.";
            }

            foreach (ItemCarrito item in items)
            {
                if (item.Cantidad <= 0)
                {
                    return "La cantidad de \"" + item.Nombre + "\" debe ser mayor que cero.";
                }

                if (item.Cantidad > item.StockDisponible)
                {
                    return "No hay stock suficiente de \"" + item.Nombre + "\" (disponible: " + item.StockDisponible + ").";
                }
            }

            decimal subtotal = CalcularSubtotal(items);

            string errorDescuento = ValidarDescuento(subtotal, descuento);
            if (errorDescuento != "")
            {
                return errorDescuento;
            }

            decimal total = subtotal - descuento;

            if (total <= 0)
            {
                return "El total de la venta debe ser mayor que cero.";
            }

            if (pagos == null || pagos.Count == 0)
            {
                return "Registra al menos un método de pago.";
            }

            decimal sumaPagos = 0;

            foreach (PagoVenta pago in pagos)
            {
                if (Array.IndexOf(MetodosPago, pago.MetodoPago) < 0)
                {
                    return "Método de pago no válido: " + pago.MetodoPago + ".";
                }

                if (pago.Monto <= 0)
                {
                    return "El monto de cada pago debe ser mayor que cero.";
                }

                if (pago.NumeroOperacion != null && pago.NumeroOperacion.Trim().Length > 100)
                {
                    return "El número de operación es demasiado largo (máximo 100 caracteres).";
                }

                sumaPagos = sumaPagos + pago.Monto;
            }

            if (Math.Round(sumaPagos, 2) != Math.Round(total, 2))
            {
                return "La suma de los pagos (S/ " + sumaPagos.ToString("0.00") + ") no coincide con el total (S/ " + total.ToString("0.00") + ").";
            }

            Venta venta = new Venta();
            venta.IdUsuario = Sesion.UsuarioActual.IdUsuario;
            venta.IdPunto = Sesion.SedeActual.IdPunto;
            venta.IdCaja = Sesion.SedeActual.IdCaja;
            venta.Subtotal = subtotal;
            venta.Descuento = descuento;
            venta.Total = total;
            venta.Items = items;
            venta.Pagos = pagos;

            try
            {
                idVenta = datos.RegistrarVenta(venta, IdTipoMovimientoVenta);
            }
            catch (InvalidOperationException ex)
            {
                // Stock insuficiente o cambiado por otra venta: ya se hizo Rollback
                idVenta = 0;
                return ex.Message;
            }

            return "";
        }

        // Lee la venta guardada y le calcula la base e IGV, para mostrar el ticket
        public Venta ObtenerVentaParaTicket(int idVenta)
        {
            Venta venta = datos.ObtenerVenta(idVenta);

            if (venta != null)
            {
                venta.BaseImponible = CalcularBaseImponible(venta.Total);
                venta.Igv = venta.Total - venta.BaseImponible;
            }

            return venta;
        }
    }
}