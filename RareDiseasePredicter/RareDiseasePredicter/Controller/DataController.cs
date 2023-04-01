using RareDiseasePredicter.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RareDiseasePredicter.Implementations;

namespace RareDiseasePredicter.Controller {

    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase {

        [HttpGet]
        [Route("/")]
        public Task<string> NoRequest() {
            return Task.FromResult("200");
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