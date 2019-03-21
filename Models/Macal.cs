using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models
{
    public class CuotaTanner
    {
        public double ValorUf { get; set; }
        public string NombreCliente { get; set; }
        public string ModeloVehiculo { get; set; }
        public string PPU { get; set; }
        public int ValorVehiculo { get; set; }
        public int Pie { get; set; }
        public int Plazo { get; set; }
        public int SaldoPrecio { get; set; }
        public int ComisionOtorgada { get; set; }
        public int GastosNotariales { get; set; }
        public int GastosPrenda { get; set; }
        public double MontoFinanciar { get; set; }
        public double SeguroDesgravamen { get; set; }
        public int Impuestos { get; set; }
        public double TasaInteres { get; set; }
        public double ValorCuota { get; set; }
        public double CAE { get; set; }
        public int CostoTotalCredito { get; set; }
        public int CostoTotalVehiculo { get; set; }
        public bool Destacado { get; set; }
        public string LegalConValores { get; set; }
        public string LegalTanner { get; set; }
        public string LegalConValoresConPie { get; set; }
        public string LegalConValoresConPieSinRentayModeloVehiculo { get; set; }
        public string LegalConValoresSinPie { get; set; }
    }

    public class Biene
    {
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string __invalid_name__Año { get; set; }
        public string Docbienurl { get; set; }
        public string Url_img { get; set; }
        public double Precio { get; set; }
        public string CajaTransmision { get; set; }
        public string Traccion { get; set; }
        public string Combustible { get; set; }
        public string Kilometraje { get; set; }
        public string PlacaUnica { get; set; }
        public string Exhibicion { get; set; }
        public bool Destacado { get; set; }
        public string Certificado_anotaciones { get; set; }
        public string Garantia_mecanica { get; set; }
        public double Garantia { get; set; }
        //public CuotaTanner CuotaTanner { get; set; }
        public string IdRemate { get; set; }
        public int Bienid { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public List<string> Imagenes { get; set; }
        public int Tipremid { get; set; }
        public int Remid { get; set; }
        public string Biendescripcionweb { get; set; }
        public int Docbienid { get; set; }
        public object Esdestacado { get; set; }
        public string Loteid { get; set; }
        public int LOTECODIGOCONTADOR { get; set; }
        public int RemCodigoContador { get; set; }
        public int Monid { get; set; }
        public string Detloteminimo { get; set; }
        public string Categoria { get; set; }
        public int LoteoDefinitivo { get; set; }
        public int NumeroLote { get; set; }
        public dynamic Detalle { get; set; }
    }

    public class Macal
    {
        public DateTime FechaRemate { get; set; }
        public object IdRemate { get; set; }
        public List<Biene> Bienes { get; set; }
    }
}
