using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class TransferResponse
    {
        public ResultModel Result { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string UserErrorDescription { get; set; }
        public int Status { get; set; }

        public class ResultModel
        {
            public string IntentosFallidosRestantes { get; set; }
            public object NumeroTransaccion { get; set; }
            public object NumeroCelular { get; set; }
            public string OtpIdTrx { get; set; }
            public string OtpCodoTp { get; set; }
            public bool PasarOtp { get; set; }
        }
    }
}
