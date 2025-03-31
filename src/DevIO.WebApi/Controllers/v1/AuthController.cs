using Asp.Versioning;
using DevIO.Business.Intefaces;
using DevIO.WebApi.Extensions;
using DevIO.WebApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DevIO.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    public class AuthController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;
        public AuthController(INotificador notificador, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, 
            IOptions<AppSettings> appSettings, IUser _AppUser, ILogger<AuthController> logger) : base(notificador, _AppUser)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _logger = logger;
        }
        /// <summary>
        /// Cria um usuário
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar(RegisterUserViewModel register)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }
            var user = new IdentityUser()
            {
                UserName = register.Email,
                Email = register.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, register.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return CustomResponse(await GerarJwt(register.Email));
            }
            foreach (var erros in result.Errors)
            {
                NotificarErro(erros.Description);
            }
            return CustomResponse(register);
        }

        /// <summary>
        /// Cria um usuário
        /// </summary>
        /// <param name="register"></param>
        /// <returns></returns>
        [HttpPost("autenticar")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUserViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return CustomResponse(ModelState);
            }
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, true);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario logado com sucesso");
                return CustomResponse(await GerarJwt(login.Email));

            }
            if (result.IsLockedOut)
            {
                NotificarErro("Usuário temporarimente bloqueado por execeder o numero de tentativas de Login");
                return CustomResponse(login);

            }
            NotificarErro("Usuario / Senha esta incorreto.");
            return CustomResponse(login);
        }

        private async Task<LoginResponseViewModel> GerarJwt(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });
            var encodedToken = tokenHandler.WriteToken(token);
            var response = new LoginResponseViewModel
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                DtExpires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        //    private static long ToUnixEpochDate(DateTime date)
        //=> (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
