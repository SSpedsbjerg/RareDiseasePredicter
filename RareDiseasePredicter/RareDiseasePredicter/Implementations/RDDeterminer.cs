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
            
            List<RDDisease> rDDiseases= new List<RDDisease>();
            List<ISymptom> dbSymptoms = (List<ISymptom>)await _symptoms;
            List<IDisease> diseases = (List<IDisease>)await _diseases;

            
            foreach (IDisease disease in diseases) {
                RDDisease rD = new RDDisease(disease);
                foreach (ISymptom diseaseSymptom in disease.Symptoms) {
                    foreach (ISymptom symptom in symptoms) {
                        if (symptom.Name == diseaseSymptom.Name) {
                            rD.Symptoms.Add(new Tuple<ISymptom, bool>(symptom,true));
                            }
                        else {
                            rD.Symptoms.Add(new Tuple<ISymptom, bool>(symptom, false));
                            }
                        }
                    }
                }

            foreach (RDDisease rDDisease in rDDiseases) {
                foreach (Tuple<ISymptom, bool> symptom in rDDisease.Symptoms) {
                    if (symptom.Item2 == true) {
                        rDDisease.Weight += 1f / rDDisease.Symptoms.Count;
                        }
                    else if (symptom.Item2 == false) {
                        rDDisease.Weight -= 1f / rDDisease.Symptoms.Count;
                        }
                    }
                }
            List<IDisease> diseasesFinal = new List<IDisease>();
            foreach (RDDisease rd in rDDiseases) {
                IDisease disease = new Disease();
                disease.Weight = rd.Weight;
                disease.Href =rd.disease.Href;
                disease.Description = rd.disease.Description;
                disease.Name = rd.disease.Name;
                disease.ID = rd.disease.ID;
                disease.Symptoms = rd.disease.Symptoms;
                diseasesFinal.Add(disease);
                }
            return diseasesFinal;
            }
        }
    }
