using AutoMapper;
using DevIO.Business.Models;
using DevIO.WebApi.ViewModel;

namespace DevIO.WebApi.Configurations
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<Fornecedor,FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();
            CreateMap<Produto, ProdutoViewModel>().ReverseMap();

           // CreateMap<Produto, ProdutoViewModel>().ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome);


        }
    }
}
