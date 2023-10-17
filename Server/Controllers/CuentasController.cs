using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PizzaBlazor.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PizzaBlazor.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager, 
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        [HttpPost("SignUp")]
        public async Task<ActionResult<UserToken>> SignUp([FromBody] UserInfo model)
        {
            var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return BuildToken(model);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserToken>> Login([FromBody] UserInfo model)
        {
            var result = await signInManager.PasswordSignInAsync(
                model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return BuildToken(model);
            }
            else
            {
                return BadRequest("Login fallido");
            }
        }

        //Método de construcción del token
        private UserToken BuildToken(UserInfo userInfo)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userInfo.UserName),
                new Claim(ClaimTypes.Email, userInfo.Email),
                new Claim("ColorFavorito", "Verde"),
                new Claim("Password", userInfo.Password), // Esto es solo para demostrar que se puede agregar cualquier claim, pero no agreguen datos sensibles
            //Ojo, no se debe agregar información sensible en los claims, ya que estos se pueden decodificar facilmente
            //Esto es meramente educativo
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(1);

            JwtSecurityToken token = new JwtSecurityToken(
                   issuer: null,
                   audience: null,
                   claims: claims,
                   expires: expiration,
                   signingCredentials: creds
                  );

            return new UserToken()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };     
        }
    }
}
