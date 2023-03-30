using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDisease {
        ICollection<ISymptom> GetSymptoms();

        int GetID();

        bool SetID(int ID);

        bool AddSymptoms(ISymptom symptom);

        string GetName();

        bool SetName(string name);
        }
    }
