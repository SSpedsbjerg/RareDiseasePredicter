using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Implementations {
    public class Region : IRegion {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private string name = "";
        private int id = -1;
        public Region(string name, int id) {
            this.name = name;
            this.id = id;
        }
    }
}
