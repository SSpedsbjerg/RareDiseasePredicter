using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<ISymptom>> getSymptoms() {
            List<ISymptom> symptoms = new List<ISymptom>();
            symptoms.Add(new Symptom("Headache"));
            symptoms.Add(new Symptom("Pain"));
            return symptoms;
        }
    }
}
