using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Models.Santander.v2
{
    public class LoginResponse
    {
        public Result Result { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public object UserErrorDescription { get; set; }
        public int Status { get; set; }
    }
}
