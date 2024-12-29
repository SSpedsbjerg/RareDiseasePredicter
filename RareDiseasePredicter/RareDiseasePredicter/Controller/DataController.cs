using RareDiseasePredicter.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RareDiseasePredicter.Implementations;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using RareDiseasePredicter.Enums;
using System.Runtime.CompilerServices;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 26/04/2023 - 28/12/2024
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


        //Takes name of symptoms and returns a list of diseases which is possible
        //TODO: Add RDDeterminer
        [HttpPost]
        [Route("/GetSuggestion/")]
        public async Task<string> GetSuggestion([FromBody]string[] symptoms) {
            string[] symptomsString = symptoms;
            RDDeterminer rDDeterminer = new RDDeterminer();
            
            List<ISymptom> _symptoms = new List<ISymptom>();
            foreach (string name in symptomsString) {
                foreach (ISymptom symp in await DatabaseController.GetSymptomsAsync()) {
                    if (symp.Name == name) {
                        _symptoms.Add(symp);
                        break;
                        }
                    }
                }
            List<IDisease> diseases = (List<IDisease>)await rDDeterminer.CalculateDiseasesAsync(_symptoms);
            string jsonString = JsonSerializer.Serialize(diseases);
            return jsonString;
            }

        List<IRegion> ToRegions(Newtonsoft.Json.Linq.JToken regions) {
            return null;
        }

        [HttpPost]
        [Route("/Register")]
        public async Task<string> AddUser([FromBody] JObject body) {
            User user = new User();
            user.role = Roles.User;
            try {
                user.Name = body.GetValue("Name").ToString();
                user.Email = body.GetValue("Email").ToString();
                user.Password = PasswordManager.HashPassword(body.GetValue("Password").ToString());
            }
            catch (Exception e){
                _ = Log.Error(e, "DataController", "AddUser, failed to extract the correct values from the JSON Object");
                return "406";
            }
            DatabaseController.SaveUser(user);
            return "200";
        }

        [HttpPost]
        [Route("/Login")]
        public async Task<string> Login([FromBody] JObject body) {
            User user = new User();
            try {
                user.Name = body.GetValue("Name").ToString();
                user.Password = PasswordManager.HashPassword(body.GetValue("Password").ToString());

            }
            catch (Exception e) {
                _ = Log.Error(e, "DataController", "Failed to verify login details");
                return "406";
            }
            user = (User)DatabaseController.GetUser(user.Name, user.Password);
            if(user is null) {
                return "401";
            }
            if(user.role == Roles.Admin) {
                return "Admin";
            }
            else if(user.role == Roles.User) {
                return "User";
            }
            return "200";
        }

        [HttpPost]
        [Route("/Disease/")]
        public async Task<string> AddDisease([FromBody] JObject body) {
            IDisease disease = null;
            try {
                disease = new Disease();
                disease.ID = -1;
                disease.Name = body.GetValue("name").ToString();
                disease.Description = body.GetValue("description").ToString();
                disease.Href = body.GetValue("href").ToString();
                disease.Symptoms = ToSymptoms(body.Value<JArray>("Symptoms"));

            }
            catch (Exception ex) {
                _= Log.Error(ex, "DataController", "HTTPPOST ADDDISEASE");
                return "500";
            }
            try {
                DatabaseController.AddDiseaseAsync(disease).Wait();
            }
            catch (AggregateException exception) {
                _ = Log.Error(exception, "DataController", "HTTPPOST ADDDISEASE, database error");
                return "500";
            }
            return "200";
        }

        List<ISymptom> ToSymptoms(JArray symptoms) {
            List<ISymptom> symptoms_ = new List<ISymptom>(); 
            foreach(JObject o in symptoms.Children<JObject>()) {
                Symptom symptom = new Symptom(); 
                symptom.Name = o.GetValue("Name").ToString();
                symptom.Description = o.GetValue("Description").ToString();
                var regs = o.Value<JArray>("Regions");
                symptom.Regions = ToRegions(regs);
                symptoms_.Add(symptom);
            }
            return symptoms_;
        }

        List<IRegion> ToRegions(JArray regions) {
            List<IRegion> regions_ = new List<IRegion>();
            foreach(JObject o in regions.Children<JObject>()) {
                IRegion region = new Region(o.GetValue("Name").ToString(), -1);
                regions_.Add(region);
            }
            return regions_;
        }

        [HttpPut]
        [Route("/Disease/")]
        public async Task<string> ModifyDisease([FromBody] JObject body) {
            IDisease disease = null;
            try {
                disease = new Disease();
                disease.ID = body["ID"].ToObject<int>();
                disease.Name = body.GetValue("Name").ToString();
                disease.Description = body.GetValue("Description").ToString();
                disease.Href = body.GetValue("Href").ToString();
                var syms = body.Value<JArray>("Symptoms");
                disease.Symptoms = ToSymptoms(syms);
            }
            catch(Exception ex) {
                _ = Log.Error(ex, "DataController", "HTTPPUT DISEASE");
                return "500";
            }
            try {
                DatabaseController.ModifyDiseaseAsync(disease).Wait();
            }
            catch(NotImplementedException exception) {
                return "501";
            }
            return "200";
        }

        [HttpPost]
        [Route("/Symptom/")]
        public async Task<string> AddSymptom([FromBody] JObject body) {
            ISymptom symptom = null;
            try {
                symptom = new Symptom();
                symptom.ID = -1;
                symptom.Name = body.GetValue("Name").ToString();
                symptom.Description = body.GetValue("Description").ToString();
                Region[] regions = body["Regions"].ToObject<Region[]>();
                foreach(Region region in regions) {
                    symptom.AddRegion(region);
                }
            }
            catch(Exception ex) {
                _ = Log.Error(ex, "DataController", "HTTPPOST ADDsymptom");
                return "500";
            }
            DatabaseController.AddSymptomAsync(symptom, -1).Wait();
            return "200";
        }

        [HttpPut]
        [Route("/Symptom/")]
        public async Task<string> ModifySymptom([FromBody] JObject body) {
            ISymptom symptom = null;
            try {
                symptom = new Symptom();
                symptom.ID = int.Parse(body.GetValue("ID").ToString());
                symptom.Name = body.GetValue("Name").ToString();
                symptom.Description = body.GetValue("Description").ToString();
                //tjek id om den er -1
                Region[] regions = body["Regions"].ToObject<Region[]>();
                foreach(Region region in regions) {
                    symptom.AddRegion(region);
                }
            }
            catch(Exception ex) {
                _ = Log.Error(ex, "DataController", "HTTPPUT Symptom");
                return "500";
            }
            try {
                DatabaseController.ModifySymptomAsync(symptom).Wait();
            }
            catch(NotImplementedException exception) {
                return "501";
            }
            return "200";
        }

        [HttpPost]
        [Route("/Region/")]
        public async Task<string> AddRegion([FromBody] JObject body) {
            IRegion region = null;
            try {
                region = new Region(body.GetValue("name").ToString() , - 1);
            }
            catch(Exception ex) {
                _ = Log.Error(ex, "DataController", "HTTPPOST ADDregion");
                return "500";
            }
            DatabaseController.AddRegionAsync(region, -1).Wait();
            return "200";
        }

        [HttpPut]
        [Route("/Region/")]
        public async Task<string> ModifyRegion([FromBody] JObject body) {
            IRegion region = null;
            try {
                region = new Region(body.GetValue("name").ToString(), -1);
                region.ID = (int)body.GetValue("id");
            }
            catch(Exception ex) {
                _ = Log.Error(ex, "DataController", "HTTPPUT Region");
                return "500";
            }
            try {
                DatabaseController.ModifyRegionAsync(region).Wait();
            }
            catch(NotImplementedException exception) {
                return "501";
            }
            return "200";
        }


        string SplitString(string value) {
            try {
                string[] nameSplit = value.Split("_");
                value = "";
                foreach(string nameSegment in nameSplit) {
                    value += $"{nameSegment} ";
                    }
                value = value.Remove(value.Length - 1);
                }
            catch {
                _ = Log.Warning("Couldn't split string", "SplitString", "");
                }
            return value;
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

        [HttpDelete]
        [Route("/Disease")]
        public async Task<string> DeleteDisease([FromHeader] JObject body) {
            return "403";
        }

        [HttpDelete]
        [Route("/Symptom")]
        public async Task<string> DeleteSymptom([FromHeader] JObject body) {
            return "403";
        }

        [HttpDelete]
        [Route("/Region")]
        public async Task<string> DeleteRegion([FromHeader] JObject body) {
            return "403";
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
            if(diseases is null)
                return "422";
            string jsonString = JsonSerializer.Serialize(diseases);
            return jsonString;
            }


        //Deprecated Methods
        [HttpGet]
        [Route("/AddRegion/{name}")]
        public async Task<string> AddRegionFromRoute([FromRoute] string name) {
            
           

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
            bool success = await DatabaseController.AddRegionAsync(new Region(name, -1), -1);
            if(success) {
                return "200";
            }
            return "500";
        }

        [HttpGet]
        [Route("/AddSymptom/{name}+{Description}+{Regions}")]
        public async Task<string> AddSymptom([FromRoute] string name, string description, string regions) {
            Task<ICollection<IRegion>> dbRegions = DatabaseController.GetRegionsAsync();
            name = SplitString(name);
            description = SplitString(description);
            ISymptom symptom = new Symptom(name);
            symptom.Description = description;
            if(!regions.StartsWith('0')) {//This should allow for symptoms without any regions
                string[] regionsArray = regions.Split(',');
                List<int> regionIDs = new List<int>();
                foreach(string region in regionsArray) {
                    regionIDs.Add(int.Parse(region));
                }
                foreach(int regionID in regionIDs) {
                    foreach(IRegion region in await dbRegions) {
                        if(regionID == region.ID) {
                            symptom.AddRegion(region);
                        }
                    }
                }
            }
            symptom.ID = -1;
            bool success = await DatabaseController.AddSymptomAsync(symptom, -1);
            if(success) {
                return "209 : Accepted, but method is deprecated";
            }
            return "500";
        }

        [HttpGet]
        [Route("/AddDisease/{name}+{description}+{href}+{symptomRef}")]
        public async Task<string> AddDisease([FromRoute] string name, string description, string href, string symptomRef) {
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
            foreach(string symptomString in symptomsArray) {
                symptomsIDs.Add(int.Parse(symptomString));
            }
            foreach(int symptomID in symptomsIDs) {
                foreach(ISymptom symptom in await dbSymptoms) {
                    if(symptomID == symptom.ID) {
                        disease.AddSymptoms(symptom);
                    }
                }
            }

            bool success = await DatabaseController.AddDiseaseAsync(disease);
            if(success) {
                return "209 : Accepted, but method is deprecated";
            }
            return "500";
        }
    }
}