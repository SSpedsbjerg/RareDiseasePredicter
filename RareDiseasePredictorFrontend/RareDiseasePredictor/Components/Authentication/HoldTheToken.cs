using System.Net.Http;
using System.Net.Http.Headers;

namespace RareDiseasePredictor.Components.Authentication

{
    public class HoldTheToken
    {
        public string Token { get; set; }
        public HttpClient httpClient = new HttpClient(); 

        public void setToken(string newToken)
        {
            Token = newToken;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }
        public HttpClient GetHttpClient()
        {
            return httpClient;
        }
    }
}
