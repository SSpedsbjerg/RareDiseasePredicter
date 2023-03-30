using System;
using System.Collections.Generic;
using System.Text;
using RareDiseasePredicter.Enums;

namespace RareDiseasePredicter.Implementations {
    class Symptom : Interfaces.ISymptom {

        private string name = "";
        List<Region> regions = new List<Region>();
        int ID = -1;


        public Symptom(string name) {
            this.name = name;
            }

        public Symptom(string name, ICollection<Region> regions) {
            this.name = name;
            this.regions = (List<Region>)regions;
            }

        public bool AddRegion(Region region) {
            regions.Add(region);
            return true;
            }

        public int GetID() {
            return ID;
        }

        public string GetName() {
            return this.name;
            }

        public ICollection<Region> GetRegions() {
            return regions;
            }

        public bool SetID(int ID) {
            this.ID = ID;
            return true;
        }

        public bool SetName(string name) {
            this.name = name;
            return true;
            }
        }
    }
