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

        public static bool AuthenticateAdmin(string username, byte[] password) {
            throw new NotImplementedException();
        }
    }
}
