using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class EstacionesParafina
    {
        public string id_estacion_servicio { get; set; }
        public string id_distribuidor { get; set; }
        public string nombre_distribuidor { get; set; }
        public string latitud { get; set; }
        public string longitud { get; set; }
        public string id_comuna { get; set; }
        public string nombre_comuna { get; set; }
        public string id_combustible { get; set; }
        public string nombre_combustible { get; set; }
        public string precio_actual { get; set; }
        public string direccion { get; set; }
        public string direccion_numero { get; set; }
        public string fecha_actualizado { get; set; }
        public string hora_actualizado { get; set; }
        public string horario_atencion { get; set; }
        public string pago_efectivo { get; set; }
        public string pago_cheque { get; set; }
        public string pago_tarjeta_tiendas { get; set; }
        public string pago_tarjeta_banco { get; set; }
    }
}
