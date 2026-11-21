using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace LaShiliquita.Utilidades
{
    // Colores y fuentes comunes para que todos los formularios se vean igual
    public static class Estilo
    {
        public static readonly Color Cafe = Color.FromArgb(59, 42, 36);
        public static readonly Color CafeClaro = Color.FromArgb(92, 67, 58);
        public static readonly Color Caramelo = Color.FromArgb(200, 135, 58);
        public static readonly Color Fondo = Color.FromArgb(246, 241, 236);
        public static readonly Color Texto = Color.FromArgb(51, 40, 36);
        public static readonly Color TextoSuave = Color.FromArgb(120, 105, 98);
        public static readonly Color Error = Color.FromArgb(185, 28, 28);

        public static Font Fuente(float tamano)
        {
            return new Font("Segoe UI", tamano);
        }

        public static Font FuenteNegrita(float tamano)
        {
            return new Font("Segoe UI", tamano, FontStyle.Bold);
        }
    }
}
