namespace RareDiseasePredictor.Models
{
    public class Disease
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Href { get; set; }
        public List<string> Symptoms { get; set; }

        public Disease()
        {
            Symptoms = new List<string>();
            Name = string.Empty;
            Description = string.Empty;
            Href = string.Empty;
        }
    }
    
}
