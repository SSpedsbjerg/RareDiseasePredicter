namespace RareDiseasePredictor.Components.Authentication
{
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using Microsoft.AspNetCore.Components.Authorization;
    using Blazored.LocalStorage;
    using Microsoft.JSInterop;
    using System.IdentityModel.Tokens.Jwt;
    using System.Runtime.CompilerServices;

    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly HoldTheToken _holder;
        private AuthenticationState anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage, HoldTheToken holder)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _holder = holder;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = null;

            try
            {
                // Attempt to get the token from singelton
                token = _holder.Token;
            }
            catch (InvalidOperationException)
            {
                // Handle the case where JSInterop is not available (e.g., during prerendering)
                return anonymousState;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return anonymousState;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            // Decode the token to extract claims
            var claims = ParseClaimsFromJwt(token);
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            return new AuthenticationState(user);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;

            if (jsonToken == null)
            {
                return null;
            }

            var claims = jsonToken.Claims;

            return claims;
        }
    }

}
