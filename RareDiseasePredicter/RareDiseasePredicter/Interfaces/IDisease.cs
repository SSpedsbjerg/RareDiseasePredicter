using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDisease {

        string Name { get; set; }

        string Description { get; set; }

        string Href { get; set; }

        int ID { get; set; }

        float Weight { get; set; }

        ICollection<ISymptom> Symptoms { get; set; }

        ICollection<float> SymptomWeigts { get; set; }

        ICollection<ISymptom> GetSymptoms();

        int GetID();

        bool SetID(int ID);

        bool AddSymptoms(ISymptom symptom);

        string GetName();

        bool SetName(string name);
        }
    }
