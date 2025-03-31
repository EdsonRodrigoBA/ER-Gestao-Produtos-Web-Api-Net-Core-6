using Asp.Versioning;
using AutoMapper;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using DevIO.Business.Services;
using DevIO.Data.Repository;
using DevIO.WebApi.Extensions;
using DevIO.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.WebApi.Controllers.v1
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _ifornecedorRepository;
        private readonly IFornecedorService _ifornecedorService;
        private readonly IMapper _imapper;
        private readonly IEnderecoRepository _enderecoRepository;

        public FornecedoresController(IFornecedorRepository ifornecedorRepository, IFornecedorService ifornecedorService, IMapper imapper, INotificador notificador, IEnderecoRepository enderecoRepository, IUser _AppUser) : base(notificador, _AppUser)
        {
            _ifornecedorRepository = ifornecedorRepository;
            _ifornecedorService = ifornecedorService;
            _imapper = imapper;
            _enderecoRepository = enderecoRepository;
        }

        [HttpGet("obter-todos")]
        public async Task<IActionResult> ObterTodos()
        {
            var fornecedores = _imapper.Map<IEnumerable<FornecedorViewModel>>(await _ifornecedorRepository.ObterTodos());
            return Ok(fornecedores);
        }

        [HttpGet("obter-por-id/{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid Id)
        {
            var fornecedor = await ObjetoFornecedorProdutosEndereco(Id);
            if (fornecedor == null)
            {
                return BadRequest();
            }
            return Ok(fornecedor);
        }
        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<EnderecoViewModel> ObterEnderecoPorId(Guid id)
        {
            return _imapper.Map<EnderecoViewModel>(await _enderecoRepository.ObterPorId(id));
        }
        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<IActionResult> Adicionar(FornecedorViewModel fornecedorViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            await _ifornecedorService.Adicionar(_imapper.Map<Fornecedor>(fornecedorViewModel));
            return CustomResponse(fornecedorViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid Id, FornecedorViewModel fornecedorViewModel)
        {
            if (Id != fornecedorViewModel.Id)
            {
                NotificarErro("O Id informado é inválido.");
                CustomResponse(fornecedorViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _ifornecedorService.Atualizar(_imapper.Map<Fornecedor>(fornecedorViewModel));
            return CustomResponse(fornecedorViewModel);

        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<IActionResult> AtualizarEndereco(Guid id, EnderecoViewModel enderecoViewModel)
        {
            if (id != enderecoViewModel.Id)
            {
                NotificarErro("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _ifornecedorService.AtualizarEndereco(_imapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }

        [ClaimsAuthorize("Fornecedor", "Excluir")]
        [HttpDelete("excluir/{id:guid}")]
        public async Task<IActionResult> Excluir(Guid Id)
        {
            var fornecedorViewModel = await ObjetoFornecedorEndereco(Id);
            if (fornecedorViewModel == null) return NotFound();
            await _ifornecedorService.Remover(Id);

            return CustomResponse();
        }

        private async Task<FornecedorViewModel> ObjetoFornecedorProdutosEndereco(Guid Id)
        {
            return _imapper.Map<FornecedorViewModel>(await _ifornecedorRepository.ObterFornecedorProdutosEndereco(Id));
        }

        private async Task<FornecedorViewModel> ObjetoFornecedorEndereco(Guid Id)
        {
            return _imapper.Map<FornecedorViewModel>(await _ifornecedorRepository.ObterFornecedorEndereco(Id));
        }
    }
}
