using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<ISymptom>> getSymptoms() {
            List<ISymptom> symptoms = new List<ISymptom>();
            symptoms.Add(new Symptom("Hovdepine", new List<Region> { Region.Head }, 0, "Smerte i hjernen"));
            symptoms.Add(new Symptom("Smerte", new List<Region> { Region.Head, Region.Face, Region.Lips, Region.Mouth, Region.Ear, Region.Neck, Region.Shoulders }, 1, "Smerte"));
            return symptoms;
            }

        //TODO: This is just for dummy data and should be connected to a database
        public static async Task<ICollection<IDisease>> getDiseases(){
            List<IDisease> diseases = new List<IDisease>();
            diseases.Add(new Disease("Steven Johnson",
                new List<ISymptom> {
                    new Symptom("Smerte",
                    new List<Region> {
                        Region.Head,
                        Region.Face,
                        Region.Lips,
                        Region.Mouth,
                        Region.Ear,
                        Region.Neck },
                    1,
                    "Smerte er lav, men over det hele" )},
                0,
                "Kommer ofte fra medicin",
                "https://www.nhs.uk/conditions/stevens-johnson-syndrome/"
                ));

            diseases.Add(new Disease("Pompes",
                new List<ISymptom> {
                    new Symptom("Muskel svaghed",
                    new List<Region> {
                        Region.UpperArms,
                        Region.LowerArms },
                    2,
                    "" )},
                1,
                "",
                ""
                ));
            return diseases;
        }
    }
}
