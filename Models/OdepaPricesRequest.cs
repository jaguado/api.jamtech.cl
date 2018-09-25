using System;
using System.Collections.Generic;
using System.Text;

namespace JAMTech.Models
{
    public class TipoSerie
    {
        public string code { get; set; }
    }

    public class Agno
    {
        public int code { get; set; }
    }

    public class SemanaInicio
    {
        public int code { get; set; }
        //public string glosa { get; set; }
    }

    public class SemanaTermino
    {
        public int code { get; set; }
        //public string glosa { get; set; }
    }

    public class Region
    {
        public int id { get; set; }
    }

    public class Sector
    {
        public int value { get; set; }
    }

    public class TipoProducto
    {
        public int id { get { return code; } }
        public int code { get; set; }
        //public string glosa { get; set; }
    }

    public class Producto
    {
        public int id { get; set; }
        public int code { get { return id; } }
        //public string glosa { get; set; }
    }

    public class TipoPuntoMonitoreo
    {
        public int id { get { return code>0 ? code : codigo; } }
        public int code { get; set; }
        public int codigo { get; set; }
        //public string glosa { get; set; }
    }

    public class Ipc
    {
        public string valor { get; set; }
        //public string glosa { get; set; }
    }

    public class OdepaPricesRequest
    {
        public TipoSerie tipoSerie { get; set; } = new TipoSerie { code = "WEEK" };
        public Agno agno { get; set; } = new Agno { code = 2018 };
        public SemanaInicio semanaInicio { get; set; } = new SemanaInicio { code = 38 };
        public SemanaTermino semanaTermino { get; set; } = new SemanaTermino { code = 38 };
        public Region region { get; set; } = new Region { id = 13 };
        public Sector sector { get; set; } = new Sector { value = -1 };
        public Producto tipoProducto { get; set; }
        public List<TipoProducto> producto { get; set; }
        public List<TipoPuntoMonitoreo> tipoPuntoMonitoreo { get; set; }
        public string tipoPeso { get; set; } = "nominales";
        public bool tipoProductoCalibreSegunda { get; set; } = true;
        public bool tipoProductoCalibrePrimera { get; set; } = true;
    }
}
