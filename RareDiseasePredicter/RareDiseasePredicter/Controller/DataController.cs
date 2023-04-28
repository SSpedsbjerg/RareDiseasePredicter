using RareDiseasePredicter.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RareDiseasePredicter.Implementations;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 26/04/2023
 * Project: RareDiseasePredictor
 * 
 */

namespace RareDiseasePredicter.Controller {

    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase {
        
        [HttpGet]
        [Route("/")]//Main page, this can be used to check connection
        public Task<string> NoRequest() {
            return Task.FromResult("200");
            }

        [HttpGet]
        [Route("/AddRegion/{name}")]
        public async Task<string> AddRegion([FromRoute]string name) {
            try {
                string[] nameSplit = name.Split("_");
                name = "";
                foreach(string nameSegment in nameSplit) {
                    name += $"{nameSegment} ";
                    }
                name = name.Remove(name.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split name of Symptom", "AddSymptom", "");
                }
            bool success = await DatabaseController.AddRegionAsync(new Region(name, -1));
            if (success) {
                return "200";
                }
            return "500";
            }

        //Takes name of symptoms and returns a list of diseases which is possible
        //TODO: Add RDDeterminer
        [HttpPost]
        [Route("/GetSuggestion/")]
        public async Task<string> GetSuggestion([FromBody]string[] symptoms) {
            string[] symptomsString = symptoms;
            List<ISymptom> _symptoms = new List<ISymptom>();
            foreach (string name in symptomsString) {
                foreach (ISymptom symp in await DatabaseController.GetSymptomsAsync()) {
                    if (symp.Name == name) {
                        _symptoms.Add(symp);
                        break;
                        }
                    }
                }
            List<IDisease> diseases = new List<IDisease>();
            foreach (ISymptom symptom_ in _symptoms) {
                foreach (IDisease disease in await DatabaseController.GetDiseaseAsync()) {
                    foreach (ISymptom symptom1 in disease.GetSymptoms()) {
                        if (symptom1.Name == symptom_.Name || symptom1.ID == symptom_.ID) {
                            diseases.Add(disease);
                            }
                        }
                    }
                }
            string jsonString = JsonSerializer.Serialize(diseases);
            return jsonString;
            }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds Disease to the database
        //TODO: Add weight for symptoms
        [HttpGet]
        [Route("/AddDisease/{name}+{description}+{href}+{symptomRef}")]
        public async Task<string> AddDisease([FromRoute]string name, string description, string href, string symptomRef) {
            //string name, List<ISymptom> symptoms, int id, string description, string href
            try {
                string[] nameSplit = name.Split("_");
                name = "";
                foreach(string nameSegment in nameSplit) {
                    name += $"{nameSegment} ";
                    }
                name = name.Remove(name.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split name of Symptom", "AddSymptom", "");
                }

            try {
                string[] descriptionSplit = description.Split("_");
                description = "";
                foreach(string descriptionSegment in descriptionSplit) {
                    description += $"{descriptionSegment} ";
                    }
                description = description.Remove(description.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split description of Symptom", "AddSymptom", "");
                }
            Task<ICollection<ISymptom>> dbSymptoms = DatabaseController.GetSymptomsAsync();
            IDisease disease = new Disease();
            disease.Name = name;
            disease.Description = description;
            disease.Href = href;
            disease.ID = -1;
            string[] symptomsArray = symptomRef.Split(',');
            List<int> symptomsIDs = new List<int>();
            foreach (string symptomString in symptomsArray) {
                symptomsIDs.Add(int.Parse(symptomString));
                }
            foreach (int symptomID in symptomsIDs) {
                foreach (ISymptom symptom in await dbSymptoms) {
                    if (symptomID == symptom.ID) {
                        disease.AddSymptoms(symptom);
                        }
                    }
                }

            bool success = await DatabaseController.AddDiseaseAsync(disease);
            if (success) {
                return "200";
                }
            return "500";
            }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds symptom to database
        [HttpGet]
        [Route("/AddSymptom/{name}+{Description}+{Regions}")]
        public async Task<string> AddSymptom([FromRoute]string name, string description, string regions) {
            Task<ICollection<IRegion>> dbRegions = DatabaseController.GetRegionsAsync();
            try {
                string[] nameSplit = name.Split("_");
                name = "";
                foreach (string nameSegment in nameSplit) {
                    name += $"{nameSegment} ";
                    }
                name = name.Remove(name.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split name of Symptom", "AddSymptom", "");
                }

            try {
                string[] descriptionSplit = description.Split("_");
                description = "";
                foreach (string descriptionSegment in descriptionSplit) {
                    description += $"{descriptionSegment} ";
                    }
                description = description.Remove(description.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split description of Symptom", "AddSymptom", "");
                }
            ISymptom symptom = new Symptom(name);
            symptom.Description = description;
            string[] regionsArray = regions.Split(',');
            List<int> regionIDs = new List<int>();
            foreach (string region in regionsArray) {
                regionIDs.Add(int.Parse(region));
                }
            foreach (int regionID in regionIDs) {
                foreach (IRegion region in await dbRegions) {
                    if (regionID == region.ID) {
                        symptom.AddRegion(region);
                        }
                    }
                }
            symptom.ID = -1;
            bool success = await DatabaseController.AddSymptomAsync(symptom);
            if(success) {
                return "200";
                }
            return "500";
            }
        
        //Get symptoms
        [HttpGet]
        [Route("/Symptoms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<string> GetSymptoms() {
            List<ISymptom> symptoms = await DatabaseController.GetSymptomsAsync() as List<ISymptom>;
            string jsonString = JsonSerializer.Serialize(symptoms);
            return jsonString;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //DEPRECATED: THIS METHOD IS NO LONGER ALLOWED BUT EXIST INCASE OF SYSTEMS DEPENDS ON IT    
        [HttpDelete]
        [Route("/Drop")]
        public async Task<string> DropTables() {
            return "403";
            }

        //Gets a list of regions
        [HttpGet]
        [Route("/Regions")]
        public async Task<string> GetRegions() {
            List<IRegion> regions = await DatabaseController.GetRegionsAsync() as List<IRegion>;
            string jsonString = JsonSerializer.Serialize(regions);
            return jsonString;
            }

        //Gets a list of diseases
        [HttpGet]
        [Route("/Diseases")]
        public async Task<string> GetDiseases() {
            List<IDisease> diseases = await DatabaseController.GetDiseaseAsync() as List<IDisease>;
            string jsonString = JsonSerializer.Serialize(diseases);
            return jsonString;
            }
        }
    }