using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Un metodo de pago de una venta (puede haber varios: pago mixto)
    public class PagoVenta
    {
        public string MetodoPago { get; set; }        // Efectivo, Yape, Plin o Tarjeta
        public decimal Monto { get; set; }
        public string NumeroOperacion { get; set; }   // opcional (Yape, Plin, Tarjeta)
    }
}