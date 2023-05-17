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

            List<IDisease> matches = new List<IDisease>();
            foreach (IDisease disease in diseases) {
                foreach (ISymptom symptom in dbSymptoms) {
                    foreach (ISymptom symp in disease.Symptoms) {
                        foreach (ISymptom sym in symptoms) {
                            if (symp.Name.Equals(sym.Name) & !matches.Contains(disease)) {
                                matches.Add(disease);
                                }
                            }
                        }
                    }
                }
            foreach (IDisease disease in matches) {
                foreach (ISymptom symptom in disease.Symptoms) {
                    foreach (ISymptom userSymptom in symptoms) {
                        if(userSymptom.Name.Equals(symptom.Name)) {
                            disease.Weight += 1.0f / symptoms.Count;
                            }
                        else {
                            disease.Weight -= 0.05f / symptoms.Count;
                            }
                        }
                    }
                }
            matches = matches.OrderByDescending(x => x.Weight).ToList();
            int index = matches.Count - 1;
            while(index != 0) {
                if(matches[index].Weight < 0) {
                    matches.Remove(matches[index]);
                    }
                index--;
                }
            if(matches[0].Weight < 0) {
                matches.Remove(matches[index]);
                }
            float totalWeight = 0;
            foreach(IDisease disease in matches) {
                totalWeight += disease.Weight;
                }

            foreach(IDisease disease in matches) {
                disease.Weight /= totalWeight;
                disease.Weight *= 100;
                }
            return matches;
            }
        }
    }

/*


*/