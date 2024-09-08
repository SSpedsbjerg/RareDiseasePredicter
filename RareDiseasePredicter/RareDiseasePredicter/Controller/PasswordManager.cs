using RareDiseasePredicter.Interfaces;
using System.Security.Cryptography;

namespace RareDiseasePredicter.Controller {
    public static class PasswordManager {
        public static bool AddNewAdmin(string username, string password) {
            throw new NotImplementedException();
        }

        private static byte[] HashPassword(string password) {
            var provider = new HMACSHA3_512();
            return provider.ComputeHash(HashPassword(password));
        }

        public static bool AuthenticateAdmin(string username, string password) {
            throw new NotImplementedException();
        }
    }
}
