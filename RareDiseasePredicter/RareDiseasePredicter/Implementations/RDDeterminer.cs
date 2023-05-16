using RareDiseasePredicter.Controller;
using RareDiseasePredicter.Interfaces;
using System.Collections;

namespace RareDiseasePredicter.Implementations {

    /**
     * 
     * This implementation uses joint probability to get the geusses
     * As there is no info on the chances of each disease with the symptoms as that would take alot of data, we give each symptom a certain risk.
     * As the number of symptoms matches, the chance increases. Diseases which dosen't match gets lowered in chance
     * 
     * */
    class RDDeterminer : IDeterminer {
        public RDDeterminer() {

            }

        public async Task<IList<IDisease>> CalculateDiseasesAsync(IList<ISymptom> symptoms) {
            var _diseases = DatabaseController.GetDiseaseAsync();
            var _symptoms = DatabaseController.GetSymptomsAsync();

            /**
             * 
             * the plan is to have each disease like the following
             * Disease: Symptom: [True/False], Symptom: [True/False] ...
             * 
             **/
            
            List<ISymptom> dbSymptoms = (List<ISymptom>)await _symptoms;
            List<IDisease> diseases = (List<IDisease>)await _diseases;

            
            foreach (IDisease disease in diseases) {
                foreach (ISymptom symptom in dbSymptoms) {
                    List<IDisease> matches = diseases.Where(p => p.Name == symptom.Name).ToList<IDisease>();
                    foreach (IDisease match in matches) {
                        if (match.Name == disease.Name) {
                            disease.Weight += 1.0f / disease.Symptoms.Count;
                            }
                        else {
                            disease.Weight -= 1.0f / disease.Symptoms.Count;
                            }
                        }
                    }
                }
            return diseases;
            }
        }
    }
