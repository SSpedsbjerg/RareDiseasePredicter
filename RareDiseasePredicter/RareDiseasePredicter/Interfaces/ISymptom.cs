using RareDiseasePredicter.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface ISymptom {
        ICollection<Region> GetRegions();

        public string Name {get;set;}

        public string Description { get;set;}

        public int ID { get; set;}

        public ICollection<Region> Regions { get; set;}

        bool AddRegion(Region region);

        }
    }
