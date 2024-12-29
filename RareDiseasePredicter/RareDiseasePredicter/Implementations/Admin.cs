using RareDiseasePredicter.Interfaces;
using System.Reflection.Metadata.Ecma335;
using RareDiseasePredicter.Controller;
using RareDiseasePredicter.Enums;

namespace RareDiseasePredicter.Implementations {
    public class Admin {
        private int permissionValue = 0;
        private string userName = "";
        private string mail = null;
        private string loggedIn;
        private string token = "";
        private int id = -1;
        byte[] password = null;
        Roles role_;
        public int PermissionLevel {
            get {
                return permissionValue;
            }
        }

        public string Name {
            get => userName;
            set => userName = value;
        }
        public string Email {
            get => mail;
            set => mail = value;
        }

        public bool IsLoggedIn => true;

        public string Token => token;

        public int ID => id;
        /*
        public string LogIn(string Username, string Password) {
            loggedIn = PasswordManager.AuthenticateUser(Username, Password);
            return loggedIn;
        }
      */
        public bool LogOut() {
;
            return true;
        }
    }
}
