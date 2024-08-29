using RareDiseasePredicter.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface IDeterminer {
        public Task<IList<Disease>> CalculateDiseasesAsync(IList<ISymptom> symptoms);
        }
    }
