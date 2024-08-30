namespace RareDiseasePredictor.Models
{
    public class ModalDataComponent
    {
        public object Modal {  get; set; }
        public string modalName { get; set; }

        public ModalDataComponent(object Modal, string modalName)
        {
            Modal = new object();
            modalName = string.Empty;
        }
    }
}
