using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Una fila del resumen de inventario de una sede para un rango de fechas
    public class ResumenInventario
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Presentacion { get; set; }

        public decimal StockInicial { get; set; }   // saldo inicial del sistema + movimientos anteriores a la fecha inicial
        public decimal Ingresadas { get; set; }     // INGRESO + PRODUCCION dentro del rango
        public decimal Vendidas { get; set; }       // VENTA dentro del rango (valor positivo)
        public decimal Salidas { get; set; }        // SALIDA dentro del rango (valor positivo)
        public decimal Ajustes { get; set; }        // AJUSTE dentro del rango (con signo)
        public decimal StockFinal { get; set; }     // stock al terminar el rango

        public decimal StockActual { get; set; }    // suma actual de inventario.cantidad en la sede
        public decimal StockMinimo { get; set; }
        public bool TieneMinimo { get; set; }       // false si productos.stock_minimo es NULL

        public bool EsStockBajo
        {
            get { return TieneMinimo && StockActual < StockMinimo; }
        }
    }
}