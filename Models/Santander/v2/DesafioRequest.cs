using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class HOST
    {
        [JsonProperty(PropertyName = "USUARIO-ALT")]
        public string UsuarioAlt { get; set; }
        [JsonProperty(PropertyName = "TERMINAL-ALT")]
        public string TerminalAlt { get; set; }
        [JsonProperty(PropertyName = "CANAL-ID")]
        public string CanalId { get; set; }
    }

    public class InfoGeneral
    {
        public string NumeroServidor { get; set; } = "20";
    }

    public class Cabecera
    {
        public HOST HOST { get; set; }
        public string CanalFisico { get; set; } = " ";
        public string CanalLogico { get; set; } = " ";
        public string RutCliente { get; set; } = "";
        public string RutUsuario { get; set; } = "";
        public string IpCliente { get; set; } = "";
        public string InfoDispositivo { get; set; } = "";
        public InfoGeneral InfoGeneral { get; set; } = new InfoGeneral();
    }

    public class Entrada
    {
        public string RutCliente { get; set; }
    }

    public class DesafioRequest
    {
        public Cabecera Cabecera { get; set; }
        public Entrada Entrada { get; set; }
    }
}
