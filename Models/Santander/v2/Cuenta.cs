using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class Cuenta
    {
        public string NroCuenta { get; set; }
        public string OficinaContrato { get; set; }
        public string CuentaDescripcion { get; set; }
        public string MontoDisponible { get; set; }
        public object MontoDisponibleUSD { get; set; }
        public object MontoAutorizadoUSD { get; set; }
        public string Moneda { get; set; }
        public string MontoUtilizado { get; set; }
        public string Cupo { get; set; }
        public string Producto { get; set; }
        public string SubProducto { get; set; }
        public string LineaDeCredito { get; set; }
        public string LineaDeCreditoAsociada { get; set; }
        public string Utilizado { get; set; }
        public string Autorizado { get; set; }
        public int Tipo { get; set; }
        public string NumeroPan { get; set; }
        public string NumeroPanE { get; set; }
        public object NumContrato { get; set; }
        public object Importe7 { get; set; }
        public object Importe9 { get; set; }
        public object Importe8 { get; set; }
        public object NombreCliente { get; set; }
        public object EstadoTarjeta { get; set; }
        public object IdRecall { get; set; }
        public object TipoCliente { get; set; }
        public object MATRIZUltMov { get; set; }
        public object MATRIZUltMovLinCre { get; set; }
        public string MPlaza { get; set; }
        public string ORetenciones { get; set; }
        public string OPlaza { get; set; }
        public string CalidadParticipacion { get; set; }
    }
}
