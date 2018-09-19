using System.Collections.Generic;

namespace JAMTech.Models.Santander
{
    public class METADATA
    {
        public string KEY { get; set; }
        public string STATUS { get; set; }
        public string DESCRIPCION { get; set; }
    }

    public class INFO
    {
        public string CODERR { get; set; }
        public string DESERR { get; set; }
        public string MSGUSUARIO { get; set; }
    }
    
    public class ESCALARES
    {
        public string NUMEROPERSONA { get; set; }
        public string TIPODOCUMENTO { get; set; }
        public string NUMERODOCUMENTO { get; set; }
        public string TIPOPERSONA { get; set; }
        public string APELLIDOPATERNO { get; set; }
        public string APELLIDOMATERNO { get; set; }
        public string NOMBREPERSONA { get; set; }
        public string NOMBREFANTASIA { get; set; }
        public string SEGMENTO { get; set; }
        public string PERFIL { get; set; }
        public string GLSEGMENTO { get; set; }
        public string SUBSEGMENTO { get; set; }
        public string GLSUBSEGMENTO { get; set; }
        public object MSGID { get; set; }
        public string LAYOUT { get; set; }
        public string MERITOLIFE { get; set; }
    }

    public class E1
    {
        public string NUMEROCONTRATO { get; set; }
        public string PRODUCTO { get; set; }
        public string SUBPRODUCTO { get; set; }
        public string MONTODISPONIBLE { get; set; }
        public string MONTOUTILIZADO { get; set; }
        public string GLOSACORTA { get; set; }
        public string OFICINACONTRATO { get; set; }
        public string CUPO { get; set; }
        public string GLOSAESTADO { get; set; }
        public string NUMEROPAN { get; set; }
        public string ESTADOOPERACION { get; set; }
        public string ESTADORELACION { get; set; }
        public string CODIGOMONEDA { get; set; }
        public string AGRUPACIONCOMERCIAL { get; set; }
    }

    public class MATRIZCAPTACIONES
    {
        public List<E1> e1 { get; set; }
    }

    public class MATRICES
    {
        public MATRIZCAPTACIONES MATRIZCAPTACIONES { get; set; }
        public object MATRIZPASIVOS { get; set; }
    }

    public class OUTPUT
    {
        public INFO INFO { get; set; }
        public ESCALARES ESCALARES { get; set; }
        public MATRICES MATRICES { get; set; }
    }

    public class DATA
    {
        public OUTPUT OUTPUT { get; set; }
    }

    public class Account
    {
        public METADATA METADATA { get; set; }
        public DATA DATA { get; set; }
    }
}
