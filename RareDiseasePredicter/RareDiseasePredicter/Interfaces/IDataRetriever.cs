namespace RareDiseasePredicter.Interfaces {
    //Reason for not making this static is if some dataretrievers have different permissions
    interface IDataRetriever {
        public Task<ICollection<IDisease>> GetDiseasesAsync();

        public Task<ICollection<ISymptom>> GetSymptomsAsync();

        public Task<ICollection<IRegion>> GetRegionsAsync();
        }
    }
