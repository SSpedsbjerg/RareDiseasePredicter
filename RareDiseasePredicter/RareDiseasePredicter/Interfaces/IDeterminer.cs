using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDeterminer {
        public Task<IList<IDisease>> CalculateDiseasesAsync(IList<ISymptom> symptoms);
        }
    }
