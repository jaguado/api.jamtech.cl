using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class VerificaRequest
    {
        public string cuentaCliente { get; set; }
        public string productoCuentaCliente { get; set; }
        public string CuentaClienteCorriente { get; set; } = "";
        public string ProductoCuentaClienteCorriente { get; set; } = "";
        public string rutDestinatario { get; set; }
        public string cuentaDestinatario { get; set; }
        public string bancoDestinatario { get; set; }
        public string tipoCuentaDestinatario { get; set; }
        public string montoTransferir { get; set; }
        public string TipoSeguridad { get; set; }
    }
}
