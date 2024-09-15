namespace RareDiseasePredictor.Components.Authentication
{
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Json;
    using Microsoft.AspNetCore.Components.Authorization;
    using Blazored.LocalStorage;
    using Microsoft.JSInterop;

    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private AuthenticationState anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string token = null;

            try
            {
                // Attempt to get the token from local storage
                token = await _localStorage.GetItemAsync<string>("authToken");
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
            var payload = jwt.Split('.')[1];
            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            var claims = keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())).ToList();

            // Extract roles if present in the token
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles))
            {
                if (roles.ToString().Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                    claims.AddRange(parsedRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                }
            }

            return claims;
        }
    }

}
