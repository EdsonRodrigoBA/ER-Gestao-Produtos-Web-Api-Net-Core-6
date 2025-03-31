using DevIO.Business.Intefaces;
using DevIO.Business.Notificacoes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DevIO.WebApi.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;
        public readonly IUser _AppUser;
        protected Guid UsuarioId;
        protected bool UsuarioAutenticado;

        public INotificador Notificador { get; }

        protected MainController(INotificador notificador, IUser appUser)
        {
            _notificador = notificador;
            _AppUser = appUser;

            if (appUser.IsAuthenticated())
            {
                UsuarioId = appUser.GetUserId();
                UsuarioAutenticado = appUser.IsAuthenticated();
            }
        }



        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacao();
        }

        protected ActionResult CustomResponse(object? result = null)
        {
            if (OperacaoValida())
            {
                return Ok(new { sucesso = true, data = result });
            }

            return BadRequest(new { sucesso = false, erros = _notificador.ObterNotificacoes().Select(m => m.Mensagem) });
        }
        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }




        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {

            var erros = modelState.Values.SelectMany(c => c.Errors);
            foreach (var erro in erros)
            {
                var erroMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(erroMsg);
            }
        }

        protected void NotificarErro(string erroMsg)
        {
            _notificador.Handle(new Notificacao(erroMsg));
        }
    }
}
