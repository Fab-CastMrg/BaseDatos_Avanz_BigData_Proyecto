using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Una fila del historial de movimientos de materia prima
    public class MovimientoMateriaPrima
    {
        public int IdMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public int IdMateriaPrima { get; set; }
        public string MateriaPrima { get; set; }
        public string UnidadMedida { get; set; }
        public string Tipo { get; set; }          // SALDO_INICIAL, COMPRA, INGRESO, SALIDA, AJUSTE, CONSUMO_PRODUCCION
        public decimal Cantidad { get; set; }     // con signo: positiva suma, negativa resta
        public string CodigoLote { get; set; }    // "" si no viene de una produccion
        public int IdCompra { get; set; }         // 0 si no viene de una compra
        public string Motivo { get; set; }
        public string Usuario { get; set; }
    }
}