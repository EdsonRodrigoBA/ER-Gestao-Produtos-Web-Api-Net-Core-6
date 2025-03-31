using Asp.Versioning;
using DevIO.Business.Intefaces;
using Elmah.Io.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.WebApi.Controllers.v2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/teste")]
    public class TesteController : MainController
    {
        private readonly ILogger _logger;

        public TesteController(INotificador notificador, IUser appUser, ILogger<TesteController> logger) : base(notificador, appUser)
        {
            _logger = logger;
        }


        [HttpGet]
        [MapToApiVersion("2.0")]
        public string ValorV2()
        {
            try
            {
                var i = 0;
                var teste = 10 / i;
            }catch(Exception ex)
            {
                ex.Ship(HttpContext);
            }

            _logger.LogTrace("Log de trace - usar em desenvolvimento");
            _logger.LogDebug("Log de debug - usar em desenvolvimento");
            _logger.LogInformation("Log de informação");
            _logger.LogWarning("log de aviso");
            _logger.LogError("Log de erro na aplicação.");
            _logger.LogCritical("Log de erro critico na aplicação.");

            return "Return V2";
        }
    }
}
