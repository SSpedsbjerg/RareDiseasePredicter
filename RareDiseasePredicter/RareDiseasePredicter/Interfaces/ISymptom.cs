using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface ISymptom {
        ICollection<Enums.Region> GetRegions();

        public string Name {get;set;}

        public string Description { get;set;}

        public int ID { get; set;}

        bool AddRegion(Enums.Region region);

        }
    }
