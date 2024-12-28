using RareDiseasePredicter.Enums;

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

        public int ID {
            get;
        }

        //This is only intended to be used for first time creation of password and authentication of the user, should be overwritten in the stack once authenticated
        public byte[] Password {
            get; set;
        }

        public Roles role {
        get; set;
        }
    }
}
