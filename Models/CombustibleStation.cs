using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class Distribuidor
    {
        public string nombre { get; set; }
        public string logo { get; set; }
        public string logo_svg { get; set; }
        public string logo_horizontal_svg { get; set; }
    }

    public class Ubicacion
    {
        public double latitud { get; set; }
        public double longitud { get; set; }
    }

    public class Servicios
    {
        public bool tienda { get; set; }
        public bool farmacia { get; set; }
        public bool mantencion { get; set; }
        public bool autoservicio { get; set; }
    }

    public class CombustibleStation
    {
        public string id { get; set; }
        public string fecha_hora_actualizacion { get; set; }
        public string razon_social { get; set; }
        public string direccion_calle { get; set; }
        public string direccion_numero { get; set; }
        public string id_comuna { get; set; }
        public string nombre_comuna { get; set; }
        public string id_region { get; set; }
        public string nombre_region { get; set; }
        public string horario_atencion { get; set; }
        public Distribuidor distribuidor { get; set; }
        public Precios precios { get; set; }
        public MetodosDePago metodos_de_pago { get; set; }
        public Ubicacion ubicacion { get; set; }
        public Servicios servicios { get; set; }
    }

    public class Precios
    {
        [JsonProperty("gasolina 93")]
        public double gasolina_93 { get; set; }
        [JsonProperty("gasolina 97")]
        public double gasolina_97 { get; set; }
        [JsonProperty("petroleo diesel")]
        public double petroleo_diesel { get; set; }
        [JsonProperty("gasolina 95")]
        public double gasolina_95 { get; set; }
        public double kerosene { get; set; }
        [JsonProperty("glp vehicular")]
        public string glp_vehicular { get; set; }
    }

    public class MetodosDePago
    {
        public bool efectivo { get; set; }
        public bool cheque { get; set; }
        [JsonProperty("tarjetas bancarias")]
        public bool tarjetas_bancarias { get; set; }
        [JsonProperty("tarjetas grandes tiendas")]
        public bool tarjetas_grandes_tiendas { get; set; }
    }

}
