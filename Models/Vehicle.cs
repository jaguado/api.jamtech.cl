using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models
{
    public class Vehicle
    {
        public string Codigo { get; set; }
        public string CodigoAnterior { get; set; }
        public double Año { get; set; }
        public string Tipo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Version { get; set; }
        public double Puertas { get; set; }
        public double Cilindrada { get; set; }
        public double Potencia { get; set; }
        public string Combustible { get; set; }
        public string Transmision { get; set; }
        public double Tasacion { get; set; }
        public double Permiso { get; set; }
    }
}
