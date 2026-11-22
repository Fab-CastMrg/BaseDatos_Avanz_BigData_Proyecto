using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaShiliquita.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreRol { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }   // texto guardado en la BD (hash); se limpia al iniciar sesion
        public bool Activo { get; set; }

        public string NombreCompleto
        {
            get { return (Nombres + " " + Apellidos).Trim(); }
        }
    }
}
