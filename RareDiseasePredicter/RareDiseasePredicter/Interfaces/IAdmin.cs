namespace RareDiseasePredicter.Interfaces {
    public interface IAdmin : IUser {
        int PermissionLevel {
            get;
        }

        string Token {
            get;
        }
    }
}
