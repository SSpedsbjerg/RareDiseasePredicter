using RareDiseasePredicter.Enums;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Implementations {
    class Symptom : ISymptom {

        private string name = "";
        private string despription = null;

        List<IRegion> regions = new List<IRegion>();
        int ID = -1;

        public string Name {
            get { return name; }
            set { this.name = value; }
            }

        public string Description {
            get { return despription; }
            set { this.despription = value; }
            }

        int ISymptom.ID {
            get { return this.ID; } 
            set { this.ID = value; }
            }

        public ICollection<IRegion> Regions {
            get { return regions; }
            set { this.regions = (List<IRegion>)value; }
            }

        public Symptom(string name) {
            this.name = name;
            }

        public Symptom(string name, ICollection<IRegion> regions) {
            this.name = name;
            this.regions = (List<IRegion>)regions;
            }

        public Symptom(string name, ICollection<IRegion> regions, int id) {
            this.name = name;
            this.regions = (List<IRegion>)regions;
            this.ID = id;
            }

        public Symptom(string name, ICollection<IRegion> regions, int id, string description) {
            this.name = name;
            this.regions = (List<IRegion>)regions;
            this.ID = id;
            this.despription = description;
            }

        public bool AddRegion(IRegion region) {
            regions.Add(region);
            return true;
            }

        public int GetID() {
            return ID;
        }

        public string GetName() {
            return this.name;
            }

        public ICollection<IRegion> GetRegions() {
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
