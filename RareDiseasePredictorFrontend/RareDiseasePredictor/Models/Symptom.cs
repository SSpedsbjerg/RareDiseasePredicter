namespace RareDiseasePredictor.Models
{
    public class Symptom
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Region> Regions { get; set; }
    }
}
