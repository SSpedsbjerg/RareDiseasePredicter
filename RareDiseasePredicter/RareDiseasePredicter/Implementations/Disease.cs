using RareDiseasePredicter.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Implementations {
    class Disease : Interfaces.IDisease {
        List<ISymptom> symptoms = new List<ISymptom>();
        string name = "";
        public Disease(string name, List<ISymptom> symptoms) {
            this.name = name;
            this.symptoms = symptoms;
        }

        public Disease() {

        }

        public bool AddSymptoms(ISymptom symptom) {
            symptoms.Add(symptom);
            return true;
        }

        public string GetName() {
            return name;
        }

        public ICollection<ISymptom> GetSymptoms() {
            return symptoms;
        }

        public bool SetName(string name) {
            this.name = name;
            return true;
        }
    }
}