using System.Numerics;

namespace RareDiseasePredictor.Models
{
    public class Disease
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Href { get; set; }
        public int ID { get; set; }
        public List<Symptom> Symptoms { get; set; }

        public Disease()
        {
            Symptoms = new List<Symptom>();
            Name = string.Empty;
            Description = string.Empty;
            Href = string.Empty;
            ID = -1;
        }
    }
    
}
