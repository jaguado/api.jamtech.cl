using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Servicios
{
    public class Persona
    {
        public string Id { get; set; } // internal id
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public Ubicacion Direccion { get; set; }
        public string CommonId { get; set; } // rut
    }
}
