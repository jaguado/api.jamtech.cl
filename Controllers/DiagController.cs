using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("v1/[controller]")]
    public class DiagController : CorsController
    {
        [HttpGet]
        public IActionResult GetDiagnostic()
        {
            return new OkObjectResult(new
            {
               
            });
        }
    }
}
