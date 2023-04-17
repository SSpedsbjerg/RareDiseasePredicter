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

        [HttpPost]
        [Route("/AddRegion")]
        public async Task<string> AddRegion([FromBody]string name) {
            Console.WriteLine(name);
            bool success = await DatabaseController.AddRegionAsync(new Region(name, -1));
            if (success) {
                return "200";
                }
            return "400";//update to fit with documentation
            }
        
        [HttpGet]
        [Route("/Symptoms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> GetSymptoms() {
            List<ISymptom> symptoms = await DatabaseController.GetSymptomsAsync() as List<ISymptom>;
            string jsonString = JsonSerializer.Serialize(symptoms);
            return jsonString;
        }

        [HttpDelete]
        [Route("/Drop")]
        public async Task<string> DropTables() {
            return "500";
            }

        [HttpGet]
        [Route("/Diseases")]
        public async Task<string> GetDiseases() {
            List<IDisease> diseases = await DatabaseController.GetDiseaseAsync() as List<IDisease>;
            string jsonString = JsonSerializer.Serialize(diseases);
            return jsonString;
            }
        }
    }