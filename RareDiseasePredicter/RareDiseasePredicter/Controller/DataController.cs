using RareDiseasePredicter.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RareDiseasePredicter.Implementations;
using System.Xml.Linq;

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

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds symptom to database
        [HttpGet]
        [Route("/AddSymptom/{name}+{Description}+{Regions}")]
        public async Task<string> AddSymptom([FromRoute]string name, string description, string regions) {
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


    //TODO: Delete before production
    [HttpGet]
        [Route("/AddDummyData")]
        public async Task<string> AddDummyData() {
            string returnstring = "200 ";
            List<IRegion> regions = new List<IRegion>();
            Region region = new Region("head", -1);//0
            regions.Add(region);
            region = new Region("left_shoulder", -1);//1
            regions.Add(region);
            region = new Region("right_shoulder", -1);//2
            regions.Add(region);
            region = new Region("left_arm", -1);//3
            regions.Add(region);
            region = new Region("right_arm", -1);//4
            regions.Add(region);
            region = new Region("chest", -1);//5
            regions.Add(region);
            region = new Region("stomach", -1);//6
            regions.Add(region);
            region = new Region("left_leg", -1);//7
            regions.Add(region);
            region = new Region("right_leg", -1);//8
            regions.Add(region);
            region = new Region("left_hand", -1);//9
            regions.Add(region);
            region = new Region("right_hand", -1);//10
            regions.Add(region);
            region = new Region("left_foot", -1);//11
            regions.Add(region);
            region = new Region("right_foot", -1);//12
            regions.Add(region);
            foreach(IRegion region_ in regions) {
                await AddRegion(region_.Name);
                returnstring += region_.Name + " ";
            }

            List<ISymptom> symptoms = new List<ISymptom>();
            Symptom symptom = new Symptom("Tinnitus");//0
            symptom.Description = "Constant ringing in the ear";
            symptom.Regions.Add(regions[0]);
            symptoms.Add(symptom);
            symptom = new Symptom("Dizziness");//1
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Balance problems");//2
            symptom.Description = "Having a hard time standing up without falling";
            symptoms.Add(symptom);
            symptom = new Symptom("Hearing loss");//3
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Paralysis");//4
            symptom.Description = "Unable to move in the region";
            for(int i = 0; i < regions.Count; i++) {
                symptom.Regions.Add((Region)regions[i]);
                }
            symptoms.Add(symptom);
            symptom = new Symptom("Numbness");//5
            symptom.Description = "null";
            for(int i = 0; i < regions.Count; i++) {
                symptom.Regions.Add((Region)regions[i]);
                }
            symptoms.Add(symptom);
            symptom = new Symptom("Tingling");//6
            symptom.Description = "null";
            for(int i = 0; i < regions.Count; i++) {
                symptom.Regions.Add((Region)regions[i]);
                }
            symptoms.Add(symptom);
            symptom = new Symptom("Hard to Swallow");//7
            symptom.Description = "null";
            symptom.Regions.Add(regions[0]);
            symptoms.Add(symptom);


            symptom = new Symptom("Coughing");//8
            symptom.Description = "null";
            symptom.Regions.Add(regions[5]);
            symptoms.Add(symptom);
            symptom = new Symptom("Breathlessness");//9
            symptom.Description = "null";
            symptom.Regions.Add(regions[5]);
            symptoms.Add(symptom);
            symptom = new Symptom("Dry coughing");//10
            symptom.Description = "null";
            symptom.Regions.Add(regions[5]);
            symptoms.Add(symptom);
            symptom = new Symptom("Fatigue");//11
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Weight loss");//12
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Chest pain");//13
            symptom.Description = "null";
            symptom.Regions.Add(regions[5]);
            symptoms.Add(symptom);
            symptom = new Symptom("Shortness of breath");//14
            symptom.Description = "null";
            symptom.Regions.Add(regions[5]);
            symptoms.Add(symptom);

            symptom = new Symptom("Diarrhea");//15
            symptom.Description = "null";
            symptom.Regions.Add(regions[6]);
            symptoms.Add(symptom);

            symptom = new Symptom("Malformation of the nails");//16
            symptom.Description = "null";
            symptom.Regions.Add(regions[9]);
            symptom.Regions.Add(regions[10]);
            symptoms.Add(symptom);
            symptom = new Symptom("Malformation of bones");//17
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Retardation");//18
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Seizures");//19
            symptom.Description = "null";
            symptoms.Add(symptom);

            symptom = new Symptom("Skin lesions");//20
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Cognitive impairment");//21
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Developmental delays");//22
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Paralysis of one side of the body");//23
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Skeletal abnormalities");//24
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Abnormal curvature of the spine");//25
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Malformation of the hip");//26
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Vomiting");//27
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Dehydration");//28
            symptom.Description= "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Muscle cramps");//29
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Weakness");//30
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Excessive Daytime Sleepiness");//31
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Loss of muscle control");//32
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Hallutinations");//33
            symptom.Description = "null";
            symptoms.Add (symptom);
            symptom = new Symptom("Sleep paralysis");//34
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Lethargy");//35
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Loss of appetite");//36
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Headache");//37
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Muscle ache");//38
            symptom.Description= "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Fever");//39
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Spasms");//40
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Nausea");//41
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Anorexia");//42
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Malaise");//43
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Rapid Heartbeat");//44
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Nervousness");//45
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Anxeity");//46
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Insomnia");//47
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Depression");//48
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Chills");//49
            symptom.Description = "null";
            symptoms.Add(symptom);
            symptom = new Symptom("Sore throat");//50
            symptom.Description = "null";
            symptoms.Add(symptom);


            //get regionsID
            regions = (List<IRegion>)await DatabaseController.GetRegionsAsync();
            foreach(ISymptom symptom_ in symptoms) {
                foreach(IRegion region_ in regions) {
                    foreach(IRegion reg in symptom_.Regions) {
                        try {
                            string[] nameSplit = reg.Name.Split("_");
                            reg.Name = "";
                            foreach(string nameSegment in nameSplit) {
                                reg.Name += $"{nameSegment} ";
                            }
                            reg.Name = reg.Name.Remove(reg.Name.Length - 1);
                            }
                        catch {
                            _ = Log.Warning("Couldn't split name of region", "DummyData", "");
                            }
                        if(region_.Name == reg.Name) {
                            reg.ID = region_.ID;
                            }
                        else if(reg.ID == -1) {
                            reg.ID = 0;
                            }
                        }
                    }
                }
                

            //List<IRegion> matches = regions.Where(p => p.Name == symptom_.Name).ToList<IRegion>();
            
            foreach(ISymptom symptom_ in symptoms) {
                string re = "";
                foreach(IRegion match in symptom_.Regions) {
                    re += match.ID + ",";
                    }
                try {
                    if (re == "") {
                        re = "0";
                        }
                    else if(re != "-1") {
                        re = re.Remove(re.Length - 1);
                        }
                    else {
                        re = "0";
                        }
                    }
                catch {
                    re = "0";
                    }
                await AddSymptom(symptom_.Name, symptom_.Description, re);
                returnstring += symptom_.Name + " ";
                }

            List<IDisease> diseases = new List<IDisease>();
            Disease disease = new Disease();//0
            disease.Description = "Acoustic neuromas are slow-growing tumors that can eventually cause a variety of symptoms by pressing against the eighth cranial nerve. Hearing loss in one ear (the ear affected by the tumor) is the initial symptom in approximately 90 percent of patients. Hearing loss is usually gradual, although in some rare cases it can be sudden. In some cases, hearing loss can also fluctuate (worsen and then improve). Hearing loss may be accompanied by ringing in the ears, a condition known as tinnitus, or by a feeling of fullness in the affected ear. In some cases, affected individuals may have difficulty understanding speech that is disproportional to the amount of hearing loss.";
            disease.Href = "https://rarediseases.org/rare-diseases/acoustic-neuroma/";
            disease.Name = "Acoustic Neuroma";
            for (int i = 0; i != 8; i++) {
                disease.AddSymptoms(symptoms[i]);
                }
            diseases.Add(disease);

            disease = new Disease();//1
            disease.Description = "Berylliosis is a form of metal poisoning caused by inhalation of beryllium dusts, vapors, or its compounds or implantation of the substance in the skin. The toxic effects of beryllium most commonly occur due to occupational exposure. Beryllium is a metallic element used in many industries, including electronics, high-technology ceramics, metals extraction, and dental alloy preparation.";
            disease.Href = "https://rarediseases.org/rare-diseases/berylliosis/";
            disease.Name = "Berylliosis";
            for(int i = 8; i != 15; i++) {
                disease.AddSymptoms(symptoms[i]);
                }
            diseases.Add(disease);

            disease = new Disease();//2
            disease.Description = "Cholera is an acute infectious disease caused by the bacterium vibrio cholerae, which lives and multiples (colonizes) in the small intestine but does not destroy or invade the intestinal tissue (noninvasive). The major symptom of cholera is massive watery diarrhea that occurs because of a toxin secreted by the bacteria that stimulates the cells of the small intestine to secrete fluid. There are several strains of V. cholerae and the severity of the disease is based on the particular infectious strain.";
            disease.Href = "https://rarediseases.org/rare-diseases/cholera/";
            disease.Name = "Cholera";
            disease.AddSymptoms(symptoms[15]);
            disease.AddSymptoms(symptoms[27]);
            disease.AddSymptoms(symptoms[28]);
            disease.AddSymptoms(symptoms[29]);
            disease.AddSymptoms(symptoms[30]);
            diseases.Add(disease);

            disease = new Disease();//3
            disease.Description = "DOOR syndrome is a rare genetic disorder that may be recognized shortly after birth. “DOOR,” an acronym for characteristic abnormalities associated with the syndrome, stands for (D)eafness due to a defect of the inner ear or auditory nerve (sensorineural hearing loss); (O)nychodystrophy or malformation of the nails; (O)steodystrophy, meaning malformation of certain bones; and mild to profound mental (R)etardation. In addition, in some cases, affected infants may have sudden episodes of uncontrolled electrical activity in the brain (seizures). Distinctive nail abnormalities may include underdeveloped, misshapen, or absent fingernails and/or toenails, while characteristic bone malformations may consist of an extra small bone in the thumbs and/or great toes (triphalangy) and/or underdevelopment (hypoplasia) of bones in other fingers and/or toes. DOOR syndrome is inherited as an autosomal recessive trait.";
            disease.Href = "https://rarediseases.org/rare-diseases/door-syndrome/";
            disease.Name = "DOOR Syndrome";
            disease.AddSymptoms(symptoms[3]);//Hearing loss
            disease.AddSymptoms(symptoms[16]);//Nail malform
            disease.AddSymptoms(symptoms[17]);//malform bones
            disease.AddSymptoms(symptoms[18]);//Retarded
            disease.AddSymptoms(symptoms[19]);//Seizures
            diseases.Add(disease);

            disease = new Disease();//4
            disease.Description = "Epidermal nevus syndromes (ENSs) are a group of rare complex disorders characterized by the presence of skin lesions known as epidermal nevi associated with additional extra-cutaneous abnormalities, most often affecting the brain, eye and skeletal systems. Epidermal nevi are overgrowths of structures and tissue of the epidermis, the outermost layer of the skin.";
            disease.Href = "https://rarediseases.org/rare-diseases/epidermal-nevus-syndromes/";
            disease.Name = "Epidermal Nevus Syndromes";
            disease.AddSymptoms(symptoms[19]);
            disease.AddSymptoms(symptoms[20]);
            disease.AddSymptoms(symptoms[21]);
            disease.AddSymptoms(symptoms[22]);
            disease.AddSymptoms(symptoms[23]);
            disease.AddSymptoms(symptoms[24]);
            disease.AddSymptoms(symptoms[25]);
            disease.AddSymptoms(symptoms[26]);
            diseases.Add(disease);

            disease = new Disease();//4
            disease.Description = "Narcolepsy is a neurological sleep disorder characterized by chronic, excessive attacks of drowsiness during the day, sometimes called excessive daytime sleepiness (EDS).";
            disease.Href = "https://rarediseases.org/rare-diseases/narcolepsy/";
            disease.Name = "Narcolepsy";
            for (int i = 30; i != 35; i++)
            {
                disease.AddSymptoms(symptoms[i]);
            }
            diseases.Add(disease);

            disease = new Disease();//5
            disease.Description = "Malaria is a communicable parasitic disorder spread through the bite of the Anopheles mosquito. ";
            disease.Href = "https://rarediseases.org/rare-diseases/malaria/";
            disease.Name = "Malaria";
            disease.AddSymptoms(symptoms[35]);
            disease.AddSymptoms(symptoms[36]);
            disease.AddSymptoms(symptoms[37]);
            disease.AddSymptoms(symptoms[38]);
            disease.AddSymptoms(symptoms[39]);
            disease.AddSymptoms(symptoms[40]);
            disease.AddSymptoms(symptoms[15]);
            disease.AddSymptoms(symptoms[27]);
            diseases.Add(disease);

            disease = new Disease();
            disease.Description = "Radiation sickness describes the harmful effects–acute, delayed, or chronic–produced by exposure to ionizing radiation.";
            disease.Href = "https://rarediseases.org/rare-diseases/radiation-sickness/";
            disease.Name = "Radiation Sickness";
            disease.AddSymptoms(symptoms[15]);
            disease.AddSymptoms(symptoms[41]);
            disease.AddSymptoms(symptoms[27]);
            disease.AddSymptoms(symptoms[42]);
            disease.AddSymptoms(symptoms[37]);
            disease.AddSymptoms(symptoms[43]);
            disease.AddSymptoms(symptoms[44]);
            diseases.Add(disease);

            disease = new Disease();
            disease.Description = "Rabies is an infectious disease that can affect all species of warmblooded animals, including man. This disorder is transmitted by the saliva of an infected animal and is caused by a virus (Neurotropic lyssavirus) that affects the salivary glands and the central nervous system.";
            disease.Href = "https://rarediseases.org/rare-diseases/rabies/";
            disease.Name = "Rabies";
            disease.AddSymptoms(symptoms[43]);
            disease.AddSymptoms(symptoms[45]);
            disease.AddSymptoms(symptoms[46]);
            disease.AddSymptoms(symptoms[47]);
            disease.AddSymptoms(symptoms[48]);
            disease.AddSymptoms(symptoms[36]);
            disease.AddSymptoms(symptoms[39]);
            disease.AddSymptoms(symptoms[49]);
            disease.AddSymptoms(symptoms[8]);
            disease.AddSymptoms(symptoms[50]);
            disease.AddSymptoms(symptoms[37]);
            disease.AddSymptoms(symptoms[41]);
            disease.AddSymptoms(symptoms[27]);
            diseases.Add(disease);

            disease = new Disease();
            disease.Description = "Schinzel syndrome, also known as ulnar-mammary syndrome, is a rare inherited disorder characterized by abnormalities of the bones of the hands and forearms in association with underdevelopment (hypoplasia) and dysfunction of certain sweat (apocrine) glands and/or the breasts (mammary glands). ";
            disease.Href = "https://rarediseases.org/rare-diseases/schinzel-syndrome/";
            disease.Name = "Schinzel Syndrome";
            disease.AddSymptoms(symptoms[17]);
            disease.AddSymptoms(symptoms[24]);
            diseases.Add(disease);


            //get symptomsID
            symptoms = (List<ISymptom>)await DatabaseController.GetSymptomsAsync();
            foreach(IDisease disease_ in diseases) {
                foreach(ISymptom symptom_ in symptoms) {
                    foreach(ISymptom sym in disease_.Symptoms) {
                        try {
                            string[] nameSplit = sym.Name.Split("_");
                            sym.Name = "";
                            foreach(string nameSegment in nameSplit) {
                                sym.Name += $"{nameSegment} ";
                                }
                            sym.Name = sym.Name.Remove(sym.Name.Length - 1);
                            }
                        catch {
                            _ = Log.Warning("Couldn't split name of symptom", "DummyData", "");
                            }
                        if(symptom_.Name == sym.Name) {
                            sym.ID = symptom_.ID;
                            }
                        else if(sym.ID == -1) {
                            sym.ID = 0;
                            }
                        }
                    }
                }


            foreach(IDisease disease_ in diseases) {
                string sy = "";
                foreach(ISymptom match in disease_.Symptoms) {
                    sy += match.ID + ",";
                    }
                try {
                    if(sy == "") {
                        sy = "0";
                        }
                    else if(sy != "-1") {
                        sy = sy.Remove(sy.Length - 1);
                        }
                    else {
                        sy = "0";
                        }
                    }
                catch {
                    sy = "0";
                    }
                await AddDisease(disease_.Name, disease_.Description, disease_.Href, sy);
                returnstring += disease_.Name + " ";
                }

            /*
            foreach (IDisease disease_ in diseases) {
                symptoms = (List<ISymptom>)await DatabaseController.GetSymptomsAsync();
                string sy = "";
                foreach(ISymptom symptom_ in symptoms) {
                    sy += symptom_.ID + ",";
                    }
                sy = sy.Remove(sy.Length - 1);
                Console.WriteLine(sy);
                await AddDisease(disease.Name, disease.Description, disease.Href, sy);
                returnstring += disease_.Name + " ";
                }*/
            return returnstring;
            }
        }
    }