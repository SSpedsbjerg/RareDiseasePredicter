using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDisease {
        ICollection<ISymptom> GetSymptoms();

        bool AddSymptoms(ISymptom symptom);
        }
    }
