namespace RareDiseasePredicter.Interfaces {

    /**
     * 
     *  --IAdminUI--
     *  
     *  All commits from a UI requires a username for fault prevention reasons.
     *  All logins requires passwords and SSO is highly disquarged
     *  IP is saved to block an IP and or to allow for easier login
     *
     */

    public interface IAdminUI {
        public string UserName { get; set; }
        public int AdminID { get; set; }

        public string IP4 { get; set; }
        public string IP6 { get; set; }


        public bool Login(string UserName, string Password);
        public IRegion AddRegion(string Name, string UserName);
        public bool RemoveRegion(string Name, string UserName);
        public bool RemoveRegion(int RegionID, string UserName);

        public ISymptom AddSymptom(string Name, IRegion Region, string UserName);
        public bool RemoveSymptom(string Name, string UserName);
        public bool RemoveSymptom(int SymptomID, string UserName);

        public IDisease AddDisease(string Name, ISymptom symptom, string UserName);
        public bool RemoveDisease(string Name, string UserName);
        public bool RemoveDisease(int DiseaseID, string UserName);

        //Should ensure a copy of the database which this queries to
        public bool ExecuteCustomSQL(string SQL, string UserName);
    }
}
