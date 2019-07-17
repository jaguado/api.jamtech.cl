using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class Result
    {
        public string KEY { get; set; }
        public string STATUS { get; set; }
        public string DESCRIPCION { get; set; }
        public object TotalDisponibleCcVista { get; set; }
        public string NumeroPersona { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public string Nombre { get; set; }
        public string ApellidoMaterno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string Segmento { get; set; }
        public string SubSegmento { get; set; }
        public string FECHACONEXION { get; set; }
        public string Logo { get; set; }
        public string Clase { get; set; }
        public List<Cuenta> cuentas { get; set; }
        public object TotalDisponibleTarjetas { get; set; }
        public string Prelife { get; set; }
        public object IdRecall { get; set; }

    }
}
