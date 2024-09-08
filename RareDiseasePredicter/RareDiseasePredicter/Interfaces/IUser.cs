namespace RareDiseasePredicter.Interfaces {
    public interface IUser {
        public string Name {
            get;
            set;
        }
        public string Email {
            get;
            set;
        }
        public bool IsLoggedIn {
            get;
        }

        public int ID {
            get;
        }


        bool LogIn(string Username, string Password);
        bool LogOut();

    }
}
