using RareDiseasePredicter.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Implementations {
    class Disease : IDisease {
        List<ISymptom> symptoms = new List<ISymptom>();
        string name = "";
        int ID = -1;
        private string description = null;
        private string href = null;

        public string Name {
            get { return name; }
            set { this.name = value; }
            }
        public string Description {
            get { return description; }
            set { this.description = value; }
            }
        public string Href {
            get { return href; }
            set { this.href = value; }
            }
        int IDisease.ID {
            get { return ID; }
            set { this.ID = value; }
            }

        ICollection<ISymptom> IDisease.symptoms {
            get { return symptoms; }
            set { this.symptoms = (List<ISymptom>)value; }
            }

        public Disease(string Name) { this.Name = Name; }

        public Disease(string name, List<ISymptom> symptoms) {
            this.name = name;
            this.symptoms = symptoms;
        }

        public Disease(string name, List<ISymptom> symptoms, int id, string description) {
            this.name = name;
            this.symptoms = symptoms;
            this.ID= id;
            this.description = description;
            }

        public Disease(string name, List<ISymptom> symptoms, int id, string description, string href) {
            this.name = name;
            this.symptoms = symptoms;
            this.ID= id;
            this.description = description;
            this.href = href;
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

        public int GetID() {
            return this.ID;
        }

        public bool SetID(int ID) {
            this.ID = ID;
            return true;
        }
    }
}