using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Una fila del historial de movimientos de inventario
    public class MovimientoInventario
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public string Tipo { get; set; }          // SALDO_INICIAL, INGRESO, SALIDA, AJUSTE, VENTA, PRODUCCION
        public decimal Cantidad { get; set; }     // con signo: positiva suma, negativa resta
        public string CodigoLote { get; set; }    // "" si no tiene lote
        public string Motivo { get; set; }
        public string Usuario { get; set; }
        public int IdVenta { get; set; }          // 0 si no viene de una venta
    }
}