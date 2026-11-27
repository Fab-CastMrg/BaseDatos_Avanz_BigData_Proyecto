using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Una linea del carrito (y del detalle de la venta)
    public class ItemCarrito
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; }   // tope de unidades que se pueden vender

        public decimal Subtotal
        {
            get { return PrecioUnitario * Cantidad; }
        }
    }
}