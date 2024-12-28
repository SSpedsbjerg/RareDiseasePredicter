using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Implementations {
    public class User : IUser {
        private string name = "";
        private string email = "";
        private int id = 0;
        private Roles roles_;
        byte[] password;

        public string Name {
            get => name;
            set => name = value;
        }
        public string Email {
            get => email;
            set => email = value;
        }

        public int ID => id;

        public byte[] Password {
            get => password;
            set => this.password = value;
        }

        public void DeposePassword() {
            password = null;
        }

        public Roles role {
            get => roles_;
            set => roles_ = value;
        }

        public bool LogIn(string Username, byte[] Password) {
            throw new NotImplementedException();
        }

        public bool LogOut() {
            throw new NotImplementedException();
        }
    }
}
