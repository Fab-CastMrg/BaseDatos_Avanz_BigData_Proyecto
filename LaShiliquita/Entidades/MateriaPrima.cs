using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    // Materia prima o insumo (cacao, azucar, empaques...) con su stock actual
    public class MateriaPrima
    {
        public int IdMateriaPrima { get; set; }
        public string Nombre { get; set; }
        public string UnidadMedida { get; set; }     // kg, g, L, ml, unidad
        public string Descripcion { get; set; }

        public decimal StockMinimo { get; set; }
        public bool TieneMinimo { get; set; }        // false si stock_minimo es NULL

        // Suma de todos sus movimientos (solo se llena al listar con stock)
        public decimal StockActual { get; set; }

        public bool EsStockBajo
        {
            get { return TieneMinimo && StockActual < StockMinimo; }
        }
    }
}