using System;
using System.Collections.Generic;

namespace LaShiliquita.Entidades
{
    // Venta completa: sirve para guardarla y para armar el ticket
    public class Venta
    {
        public int IdVenta { get; set; }          // es el numero de ticket
        public int IdUsuario { get; set; }
        public int IdPunto { get; set; }
        public int IdCaja { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }        // = Subtotal - Descuento (CHECK de la BD)

        // Valores derivados (no se guardan; los calcula VentaNegocio)
        public decimal BaseImponible { get; set; }
        public decimal Igv { get; set; }

        // Solo para mostrar en el ticket
        public string NombreVendedor { get; set; }
        public string NombreSede { get; set; }

        public List<ItemCarrito> Items { get; set; }
        public List<PagoVenta> Pagos { get; set; }

        public Venta()
        {
            Items = new List<ItemCarrito>();
            Pagos = new List<PagoVenta>();
        }
    }
}