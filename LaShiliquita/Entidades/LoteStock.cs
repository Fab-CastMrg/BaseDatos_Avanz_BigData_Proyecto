using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace LaShiliquita.Entidades
{
    // Lote de un producto, con el stock que tiene en una sede (para elegirlo en un ingreso)
    public class LoteStock
    {
        public int IdLote { get; set; }            // 0 = "sin lote" (lo agrega la pantalla)
        public string CodigoLote { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal StockEnSede { get; set; }

        // Texto para mostrar en una lista desplegable
        public string Texto
        {
            get
            {
                if (IdLote == 0)
                {
                    return CodigoLote;
                }

                string vence = FechaVencimiento.HasValue
                    ? FechaVencimiento.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                    : "sin vencimiento";

                return CodigoLote + "  (vence " + vence + ", stock en sede: " +
                       StockEnSede.ToString("0.##", CultureInfo.InvariantCulture) + ")";
            }
        }
    }
}
