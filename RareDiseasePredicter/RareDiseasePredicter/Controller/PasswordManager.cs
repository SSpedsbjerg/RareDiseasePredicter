using RareDiseasePredicter.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace RareDiseasePredicter.Controller {
    public static class PasswordManager {


        public static bool AddNewAdmin(string username, string password) {
            throw new NotImplementedException();
        }

        public static byte[] HashPassword(string password) {
            var provider = new HMACSHA512();
            provider.Key = Encoding.UTF8.GetBytes("verysecretkeynobodyknowsevenobamadoesntknowthisone");
            return provider.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        //For at checke om at token er validt, kan man gemme det i en værdi (array eller andet) og sammenligne den værdi med det token som bliver sendt fra http requesten. Listen kan tømmes ved at checke for Delta tid og fjerne udgået tokens.
        public static string AuthenticateUser(string username, string password, IConfiguration _configuration) {
            AuthService auth = new AuthService(_configuration);

            //TODO  REMOVE ALL THIS SHIT TEST DATA VALUES AND IMPLEMENT USERNAME & PASSWORD CHECKERS YEP
            string[] roles = ["Admin"];
            Console.WriteLine("Attempting to generate Token... ");


            string token = auth.GenerateJwtToken(username, roles);

            return token;
        }
    }
}
