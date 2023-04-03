using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDisease {

        string Name { get; set; }

        string Description { get; set; }

        string Href { get; set; }

        int ID { get; set; }

        ICollection<ISymptom> symptoms { get; set; }

        ICollection<ISymptom> GetSymptoms();

        int GetID();

        bool SetID(int ID);

        bool AddSymptoms(ISymptom symptom);

        string GetName();

        bool SetName(string name);
        }
    }
