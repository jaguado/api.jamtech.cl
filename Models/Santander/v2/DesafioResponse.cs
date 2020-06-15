using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class METADATA
    {
        public string STATUS { get; set; }
        public string DESCRIPCION { get; set; }
    }

    public class Informacion
    {
        public string Codigo { get; set; }
        public string Resultado { get; set; }
        public string Mensaje { get; set; }
    }

    public class Escalares
    {
        public string NumeroTarjeta { get; set; }
        public object TipoAutentificacion { get; set; }
        public object GlosaTipoAutentificacion { get; set; }
        public string MatrizDesafio { get; set; }
    }

    public class DATA
    {
        public Informacion Informacion { get; set; }
        public Escalares Escalares { get; set; }
    }

    public class DesafioResponse
    {
        public METADATA METADATA { get; set; }
        public DATA DATA { get; set; }
    }
}
