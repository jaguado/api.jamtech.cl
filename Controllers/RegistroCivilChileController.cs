using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace JAMTech.Controllers
{
    /// <summary>
    /// Encapsulate and provides access to online service on RegistroCivil.cl
    /// </summary>
    [Route("v1/[controller]")]
    public class RegistroCivilChileController : Controller
    {
        const string url = @"https://portal.sidiv.registrocivil.cl/usuarios-portal/pages/DocumentRequestStatus.xhtml?RUN={0}&type={2}&serial={1}";
        const string resultTableId = "tableResult";

        /// <summary>
        /// GET Chilean ID state
        /// </summary>
        /// <param name="chileanId">Chilean RUT (XXXXXXXX-X)</param>
        /// <param name="chileanIdSerialNumber">Chilean ID Serial number</param>
        /// <param name="idType">Kind of Chilean ID</param>
        /// <returns>ID State</returns>
        [HttpGet]
        public async Task<IActionResult> CheckState(string chileanId, string chileanIdSerialNumber, IdType idType=IdType.CEDULA)
        {
            var result = await GetIdState(chileanId, chileanIdSerialNumber, idType);
            if (result == null)
                return new NotFoundResult();
            return new OkObjectResult(result);
        }

        private static async Task<string> GetIdState(string chileanId, string chileanIdSerialNumber, IdType type)
        {
            //work around to avoid problems with some valide certificates on linux -> https://github.com/dotnet/corefx/issues/21429
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        
            var tempUrl = string.Format(url, chileanId.Replace(".",""), chileanIdSerialNumber, Enum.GetName(typeof(IdType), type));
            var responseBody = await new HttpClient().GetStringAsync(tempUrl);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(responseBody);

            var columns = doc.DocumentNode.Descendants().Where
                    (x => (x.Name == "table" && x.Attributes["id"] != null &&
                       x.Attributes["id"].Value.Equals(resultTableId))).Single().Descendants("td").ToList();
            if (columns.Count == 2) return columns[1].InnerText;
            return null;
        }

        public enum IdType
        {
            CEDULA, CEDULA_EXT, PASAPORTE_PG, PASAPORTE_DIPLOMATICO, PASAPORTE_OFICIAL
        }
    }
}