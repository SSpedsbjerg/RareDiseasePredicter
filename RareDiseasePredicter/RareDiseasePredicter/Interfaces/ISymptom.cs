using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface ISymptom {
        ICollection<Enums.Region> GetRegions();

        int GetID();

        bool SetID(int ID);

        bool AddRegion(Enums.Region region);

        string GetName();

        bool SetName(string name);

        }
    }
