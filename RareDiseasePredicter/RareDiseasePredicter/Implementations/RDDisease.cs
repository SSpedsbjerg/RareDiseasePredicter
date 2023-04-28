using RareDiseasePredicter.Interfaces;
using System.Runtime.CompilerServices;

namespace RareDiseasePredicter.Implementations {
    class RDDisease {
        private IDisease disease1 = null;
        IList<Tuple<ISymptom, bool>> symptoms;
        float weight = 0f;
        public RDDisease(IDisease disease) {

            }
        public IDisease disease { get { return disease1; } set { disease1 = value; } }
        public IList<Tuple<ISymptom, bool>> Symptoms { get { return symptoms; } set { symptoms = value; } }
        public float Weight { get { return weight; } set { weight = value; } }
        }
    }
