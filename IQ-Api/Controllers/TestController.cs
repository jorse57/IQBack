using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IQ_Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<string> apiConexion()
        {
           
            try
            {
                
                    string respuesta = "Conexión a API establecida correctamente.";

                    return respuesta;

            }
            catch (Exception)
            {

                throw;
            }

        }


    }
}
