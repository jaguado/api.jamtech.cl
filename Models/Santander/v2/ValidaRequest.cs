using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{    public class ValidaRequest
    {
        public string CuentaCliente { get; set; }
        public string ProductoCuentaCliente { get; set; }
        public string EmailCliente { get; set; }
        public string RutDestinatario { get; set; }
        public string CuentaDestinatario { get; set; }
        public string ProductoCuentaDestinatario { get; set; }
        public string BancoDestinatario { get; set; }
        public string NombreBancoDestinatario { get; set; }
        public string Alias { get; set; }
        public string NombreDestinatario { get; set; }
        public string EmailDestinatario { get; set; }
        public string ComentarioEmail { get; set; }
        public string MatrizDesafio { get; set; }
        public string MontoMaximoTransferir { get; set; }
        public string MontoTransferir { get; set; }
    }
}
