using RareDiseasePredicter.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RareDiseasePredicter.Implementations;

namespace RareDiseasePredicter.Controller {

    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase {

        internal class JSONFormatter {

            }

        [HttpGet]
        [Route("/")]
        public async Task<string> test() {
            return "200";
            }

        [HttpGet]
        [Route("/Symptoms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> GetSymptoms() {            
            List<ISymptom> symptoms = new List<ISymptom>();
            List<string> stringSymptoms = new List<string>();
            symptoms = await DatabaseController.getSymptoms() as List<ISymptom>;

            string jsonString = JsonSerializer.Serialize(symptoms);
            return jsonString;
        }
    }
}