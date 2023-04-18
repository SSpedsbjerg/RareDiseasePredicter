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
        [Route("/AddRegion/{name}")]
        public async Task<string> AddRegion([FromRoute]string name) {
            Console.WriteLine(name);
            bool success = await DatabaseController.AddRegionAsync(new Region(name, -1));
            if (success) {
                return name;
                }
            return name;//update to fit with documentation
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
        [Route("/Regions")]
        public async Task<string> GetRegions() {
            List<IRegion> regions = await DatabaseController.GetRegionsAsync() as List<IRegion>;
            string jsonString = JsonSerializer.Serialize(regions);
            return jsonString;
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