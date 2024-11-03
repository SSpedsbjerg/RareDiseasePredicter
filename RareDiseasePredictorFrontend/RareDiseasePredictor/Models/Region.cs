namespace RareDiseasePredictor.Models
{
    public class Region
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public Region() 
        {
            Name = string.Empty;
            ID = -1;
        }
    }
}
