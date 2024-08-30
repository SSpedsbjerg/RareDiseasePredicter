namespace RareDiseasePredictor.Models
{
    public class Symptom
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public List<Region> Regions { get; set; }
        public Symptom()
        {
            Name = string.Empty;
            Description = string.Empty;
            Id = -1;
            Regions = new List<Region>();

        }
    }
}
