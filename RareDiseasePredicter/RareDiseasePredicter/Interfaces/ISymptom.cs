using RareDiseasePredicter.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RareDiseasePredicter.Interfaces {
    interface ISymptom {
        ICollection<IRegion> GetRegions();

        public string Name {get;set;}

        public string Description { get;set;}

        public int ID { get; set;}

        public ICollection<IRegion> Regions { get; set;}

        bool AddRegion(IRegion region);

        }
    }
