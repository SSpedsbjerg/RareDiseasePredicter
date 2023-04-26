using Microsoft.Data.Sqlite;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 26/04/2023
 * Project: RareDiseasePredictor
 * 
 */

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        private static SqliteConnection Connection;

        public static bool Start() {
            if (!CreateConnection()) return false;
            if (!CreateTables()) return false;
            return true;
            }

        private static bool CreateConnection() {
            if (Connection == null) {
                try {
                    Connection = new SqliteConnection("Data Source = Database.db");
                    Connection.Open();
                    return true;
                }
                catch(Exception e) {
                    _ = Log.Error(e, "CreateConnection", e.Message);
                    return false;
                }
            }
            else {
                return true;
            }
        }

        //TODO: add weight to disease
        private static bool CreateTables() {
            try {
                string createQuery;
                createQuery = "CREATE TABLE IF NOT EXISTS DiseaseSymptomsReference " +
                    "(ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "DiseaseID INTEGER NOT NULL, " +
                    "SymptomID INTEGER NOT NULL);";
                var command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Disease" +
                    " (ID INTEGER PRIMARY KEY AUTOINCREMENT," +
                    " Description TEXT," +
                    " Href TEXT," +
                    " Name TEXT" +
                    " );";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS RegionSymptoms (" +
                    "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "Symptom INTEGER NOT NULL, " +
                    "Region INTEGER NOT NULL);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Regions (" +
                    " ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " Name TEXT);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                //TODO: Update Region to reference RegionSymptoms
                createQuery = "CREATE TABLE IF NOT EXISTS Symptoms (" +
                    " ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " Region INTEGER NOT NULL," +
                    " Name TEXT," +
                    " Description TEXT" +
                    " );";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();
                return true;
            }
            catch {
                return false;
            }

        }

        //Gets all diseases along with the symptoms and regions
        //TODO: See if the complexity can be improved
        public static async Task<ICollection<IDisease>> GetDiseaseAsync() {
            List<IDisease> diseaseList = new List<IDisease>();
            if(CreateConnection()) {
                //Get all diseases
                var command = Connection.CreateCommand();
                command.CommandText = "SELECT * FROM Disease";
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        string href = reader.GetString(2); //Href
                        int id = reader.GetInt32(0); //ID
                        string description = reader.GetString(1); //Description
                        string name = reader.GetString(3); //Name
                        var disease = new Disease();
                        disease.SetID(id);
                        disease.Name = name;
                        disease.Href = href;
                        disease.Description = description;
                        diseaseList.Add(disease);
                        }
                    }
                //Get their symptoms
                command.CommandText = "SELECT * FROM DiseaseSymptomsReference";
                List<ISymptom> symptoms = (List<ISymptom>)await GetSymptomsAsync(); //This gets the symptoms regions aswell
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        foreach(IDisease disease in diseaseList) {
                            if(reader.GetInt32(1) == disease.ID) { //DiseaseSymoptomsReference DiseaseID
                                foreach(ISymptom symptom in symptoms) {
                                    if(symptom.ID == reader.GetInt32(2)) { //DiseaseSymoptomsReference SymptomID
                                        disease.AddSymptoms(symptom);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            else {
                _ = Log.Error(new Exception("Could not create Connection"), "GetSymptomsAsync", "");
                return null;
                }
            return diseaseList;
            }

        //Gets all symptoms alongside its regions
        //TODO: see if complexity can be improved
        public static async Task<ICollection<ISymptom>> GetSymptomsAsync() {
            List<ISymptom> symptoms = new List<ISymptom>();
            if(CreateConnection()) {
                var command = Connection.CreateCommand();
                command.CommandText = "SELECT * FROM Symptoms";
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        string name = reader.GetString(2);
                        int id = reader.GetInt32(0);
                        string description = reader.GetString(3);
                        ISymptom symptom = new Symptom(name) {
                            Description = description
                            };
                        symptom.ID = id;
                        symptoms.Add(symptom); //Adds symptom
                        }
                    }
                command.CommandText = "SELECT * FROM RegionSymptoms";
                List<IRegion> regions = (List<IRegion>)await GetRegionsAsync();
                using (var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        foreach(ISymptom symptom in symptoms) {//search the symptoms
                            if(reader.GetInt32(1) == symptom.ID) {//if a symptomreference matches with the symptoms' id  continue
                                foreach(IRegion region in regions) {//search for all regions
                                    if(region.ID == reader.GetInt32(2)) {//if a regions id matches with any the regions references add the region to the symptom
                                        symptom.AddRegion(region);
                                        }
                                    }
                                }
                            }
                        }
                    }
            }
            else {
                _ = Log.Error(new Exception("Could not create Connection"), "GetSymptomsAsync", "");
                return null;
            }
            return symptoms;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds disease to the database
        public static async Task<bool> AddDiseaseAsync(IDisease disease) {
            var command = Connection.CreateCommand();
            if(CreateConnection()) {
                command.CommandText = "SELECT ID FROM Disease";
                using (var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        disease.ID = reader.GetInt32(0) + 1;
                        }
                    }
                command.CommandText = $"INSERT INTO Disease (Description, Href, Name) VALUES ('{disease.Description}', '{disease.Href}', '{disease.Name}');";
                command.ExecuteNonQuery();
                foreach (ISymptom symptom in disease.Symptoms) {
                    command.CommandText = $"INSERT INTO DiseaseSymptomsReference (DiseaseID, SymptomID) VALUES ({disease.ID}, {symptom.ID});";
                    }
                command.ExecuteNonQuery();
                }
            else {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddDiseaseAsync", "");
                return false;
            }
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds symptom to the database
        public static async Task<bool> AddSymptomAsync(ISymptom symptom) {
            if (!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddSymptomAsync", "");
                return false;
                }
            try {
                var command = Connection.CreateCommand();
                command.CommandText = "SELECT Symptom FROM RegionSymptoms";//Get the reference of regions for the symptom
                int lastRefID = -1;
                bool hasData = false;
                using(var reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        hasData = true;
                        lastRefID = reader.GetInt32(0);
                    }
                    if (!hasData) {//If the database is empty
                        lastRefID= 0;
                        }
                }
                if (lastRefID == -1 ) {//It should not end in here
                    _=Log.Error(new Exception($"lasRefID was {lastRefID}"), "AddSymptomAsync", "");
                    return false;
                    }
                lastRefID++;
                command.CommandText = $"INSERT INTO Symptoms (Region, Name, Description) VALUES ('{lastRefID}', '{symptom.Name}', '{symptom.Description}')";
                var query = command.ExecuteNonQueryAsync();
                foreach (IRegion region in symptom.Regions) {
                        await AddSympRegionReferenceAsync(lastRefID, region.ID);
                    }
                await query;
                return true;
            }
            catch (Exception ex) {
                _ = Log.Error(ex, "AddSymptomAsync", "");
                return false;
            }
        }

        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddSympRegionReferenceAsync", "");
                return false;
                }
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM RegionSymptoms";
            command.CommandText = $"INSERT INTO RegionSymptoms (Symptom, Region) VALUES ({sympID}, {regionID})";
            command.ExecuteNonQuery();
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Read all of the regions and if any matches, don't add it
        //possibility of wrong IDs comes from this, if any mismatch with regions pops up, check this
        public static async Task<bool> AddRegionAsync(IRegion region) {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddRegionAsync", "");
                return false;
                }
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Regions";
            using (var reader = command.ExecuteReader()) {
                while(await reader.ReadAsync()) {
                    if (reader.GetString(0).ToLower() == region.Name.ToLower()) {
                        _ = Log.Warning("Tried to add a Region that already exist", "AddRegionAsync", "");
                        return false;
                    }
                }
            }
            command.CommandText = $"INSERT INTO Regions (Name) VALUES ('{region.Name}');";
            command.ExecuteNonQuery();
            return true;
            }

        //Gets regions
        public static async Task<ICollection<IRegion>> GetRegionsAsync() {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection"), "GetRegionsAsync", "");
                return null;
                }
            List<IRegion> regions = new List<IRegion>();
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM Regions";
            using (var reader = command.ExecuteReader()) {  
                while(await reader.ReadAsync()) {
                    regions.Add(new Region(reader.GetString(1), reader.GetInt32(0)));
                }
            }
            return regions;
        }
    }
}
