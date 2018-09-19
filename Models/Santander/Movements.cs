using System.Collections.Generic;

namespace JAMTech.Models.Santander.Movement
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
        public string CCC { get; set; }
        public string Divisa { get; set; }
        public string FecDesde { get; set; }
        public string FecHasta { get; set; }
        public string MoviDesde { get; set; }
        public string MoviHasta { get; set; }
        public string TipRegistro { get; set; }
        public string Timestamp { get; set; }
        public string RegRecuper { get; set; }
        public string CobrarComision { get; set; }
        public string Cartola { get; set; }
        public string Informados { get; set; }
    }

    public class MovimientosDeposito
    {
        public string TipReg { get; set; }
        public string NumMov { get; set; }
        public string Codigo { get; set; }
        public string Importe { get; set; }
        public string Concepto { get; set; }
        public string FechConta { get; set; }
        public string FechOper { get; set; }
        public string HoraOper { get; set; }
        public string FechValor { get; set; }
        public string Cheque { get; set; }
        public string NuevoSaldo { get; set; }
        public string IndAomo { get; set; }
        public string IndAct { get; set; }
        public string IndAnulab { get; set; }
        public string IndAnulado { get; set; }
        public string IndCargabo { get; set; }
        public string IndGiro { get; set; }
        public string Codigur { get; set; }
        public string TipoCambio { get; set; }
        public string Canal { get; set; }
        public string IndPpal { get; set; }
        public string Transaccion { get; set; }
        public string CodAplicac { get; set; }
        public object TermNio { get; set; }
        public string FechNio { get; set; }
        public string HoraNio { get; set; }
        public string ConcepAmp { get; set; }
        public string CodigoAmp { get; set; }
        public string EntidadUmo { get; set; }
        public string CentroUmo { get; set; }
        public string UseridUmo { get; set; }
        public string CajeroUmo { get; set; }
        public object NetnaneUmo { get; set; }
        public string Observa { get; set; }
        public string ReferMvto { get; set; }
        public string TipDep { get; set; }
        public string DivDep { get; set; }
        public string CentroDep { get; set; }
        public string NumPapeleta { get; set; }
        public string IndRetEsp { get; set; }
        public string TimeUmo { get; set; }
    }

    public class DATA
    {
        public Informacion Informacion { get; set; }
        public Escalares Escalares { get; set; }
        public List<MovimientosDeposito> MovimientosDepositos { get; set; }
    }

    public class Movements
    {
        public METADATA METADATA { get; set; }
        public DATA DATA { get; set; }
    }
}
