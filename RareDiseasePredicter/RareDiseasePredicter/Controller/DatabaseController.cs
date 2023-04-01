using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<ISymptom>> getSymptoms() {
            List<ISymptom> symptoms = new List<ISymptom>();
            symptoms.Add(new Symptom("Headache", new List<Region> { Region.Head }, 0, "Pain in Brain"));
            symptoms.Add(new Symptom("Pain", new List<Region> { Region.Head, Region.Face, Region.Lips, Region.Mouth, Region.Ear, Region.Neck, Region.Shoulders }, 1, "Pain"));
            return symptoms;
        }
    }
}
