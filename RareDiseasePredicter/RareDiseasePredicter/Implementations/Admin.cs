using RareDiseasePredicter.Interfaces;
using System.Reflection.Metadata.Ecma335;
using RareDiseasePredicter.Controller;
using RareDiseasePredicter.Enums;

namespace RareDiseasePredicter.Implementations {
    public class Admin : IAdmin {
        private int permissionValue = 0;
        private string userName = "";
        private string mail = null;
        private bool loggedIn = false;
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

        public bool IsLoggedIn => loggedIn;

        public string Token => token;

        public int ID => id;

        public byte[] Password {
            get => password;
            set => password = value;
        }
        public Roles role {
            get => role_;
            set => role_ = value;
        }

        public bool LogIn(string Username, string Password) {
            return LogIn(Username, PasswordManager.HashPassword(Password));
        }

        public bool LogIn(string Username, byte[] Password) {
            loggedIn = PasswordManager.AuthenticateAdmin(Username, Password);
            return loggedIn;
        }

        public bool LogOut() {
            loggedIn = false;
            return true;
        }
    }
}
