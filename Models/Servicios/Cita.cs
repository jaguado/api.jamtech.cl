using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Servicios
{
    public class Cita
    {
        public string Id { get; set; }
        public Proveedor Proveedor { get; set; }
        public Cliente Cliente { get; set; }
        public Servicio Servicio { get; set; }
        public DateTime Cuando { get; set; }
        public Ubicacion Donde { get; set; }
        public List<Tuple<DateTime, string>> Comentarios { get; set; }
        public List<Pago> Pagos { get; set; }
        public Estados Estado { get; set; }
        public List<Tuple<DateTime, Estados>> Historia { get; set; } // cambios de estado
        public List<Tuple<DateTime, string>> Antecedentes { get; set; } // interno para cambios de datos y otras cosas
    }
    
    public class UserCita
    {
        public string uid { get; set; }
        public IEnumerable<Cita> Data { get; set; }

        public id _id { get; set; }
        public class id
        {
            [JsonProperty("$oid")]
            public string oid { get; set; }
        }
    }
}
