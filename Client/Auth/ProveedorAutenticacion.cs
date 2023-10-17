using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PizzaBlazor.Client.Helpers;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PizzaBlazor.Client.Auth
{
    public class ProveedorAutenticacion : AuthenticationStateProvider, ILoginService
    {
        private readonly IJSRuntime js;
        private readonly HttpClient client;

        public ProveedorAutenticacion(IJSRuntime js, HttpClient client)
        {
            this.js = js;
            this.client = client;
        }

        public static readonly string TokenKey = "token";
        private AuthenticationState Anonimo => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await js.ObtenerLocalStorage(TokenKey);
            if (token is null)
            {
                return Anonimo;
            }
            return ConstruirAuthenticationState(token.ToString());
        }

        private AuthenticationState ConstruirAuthenticationState(string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            var claims = ObtenerClaims(token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        private IEnumerable<Claim> ObtenerClaims(string token)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenDeserializado = jwtSecurityTokenHandler.ReadJwtToken(token);
            return tokenDeserializado.Claims;
        }

        public async Task Login(string token)
        {
            await js.GuardarLocalStorage(TokenKey, token);
            var authState = ConstruirAuthenticationState(token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task Logout()
        {
            await js.RemoverLocalStorage(TokenKey);
            client.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));
        }
    }
}
