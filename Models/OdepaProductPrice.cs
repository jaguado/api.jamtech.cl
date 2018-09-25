using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class OdepaProductPrice
    {
        public int semana { get; set; }
        public string fechaInicio { get; set; }
        public string fechaTermino { get; set; }
        public string tipoPuntoMonitoreo { get; set; }
        public string producto { get; set; }
        public string unidad { get; set; }
        public double precioMinimo { get; set; }
        public double precioMaximo { get; set; }
        public double precioPromedio { get; set; }
    }
}
