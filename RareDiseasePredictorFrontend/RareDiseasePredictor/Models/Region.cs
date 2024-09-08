namespace RareDiseasePredictor.Models
{
    public class Region
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public Region() 
        {
            Name = string.Empty;
            Id = -1;
        }
    }
}
