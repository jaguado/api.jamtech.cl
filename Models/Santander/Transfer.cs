using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander
{
    public class Transfer
    {
        public BankCredentials Credentials { get; set; }
        public string CuentaCliente { get; set; }
        public string EmailCliente { get; set; }
        public string Alias { get; set; } = "Cuenta Corriente";
        public string RutDestinatario { get; set; }
        public string CuentaDestinatario { get; set; }
        public string BancoDestinatario { get; set; }
        public string NombreBancoDestinatario { get; set; }
        public string NombreDestinatario { get; set; }
        public string EmailDestinatario { get; set; }
        public string ComentarioEmail { get; set; }
        public string MontoTransferir { get; set; }
    }
}
