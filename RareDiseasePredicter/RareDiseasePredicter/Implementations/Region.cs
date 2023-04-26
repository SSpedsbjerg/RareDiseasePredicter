using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Implementations {
    public class Region : IRegion {
        public string Name { get { return name; } set { this.name = value; } }
        public int ID { get { return id; } set { this.id = value; } }

        private string name = "";
        private int id = -1;
        public Region(string name, int id) {
            this.name = name;
            this.id = id;
        }
    }
}
