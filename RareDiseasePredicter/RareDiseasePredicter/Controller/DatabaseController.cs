using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;
using MySql.Data;
using MySql.Data.MySqlClient;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 15/09/2024
 * Project: RareDiseasePredictor
 * 
 */

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {


        private static string Server = "localhost";
        private static string DatabaseName = "Local instance MySQL84";
        private static string port = "3307";
        private static string userName;
        private static string password;

        public static string UserName {
            get {
                return userName;
            }
            set {
                userName = value;
            } 
        }

        public static string Password {
            private get {
                return password;
            }
            set {
                password = value;
            }
        }

        private static MySqlConnection connection;

        public static bool isConnected = false;

        public static bool ConnectDatabase() {
            if(connection is null) {
                try {
                    string connstring = string.Format("Server={0}; port={1}; database={1}; UID={2}; password={3}", Server, port, DatabaseName, UserName, Password);
                    connection = new MySqlConnection(connstring);
                    connection.Open();
                    isConnected = true;
                    return true;
                }
                catch {
                    isConnected = false;
                    _ = Log.Error(new Exception("Unable to open connection to MYSQL database"), "DatabaseController", "ConnectDatabase");
                }
            }
            return false;
        }

        public static void CloseDatabase() {
            if(connection is null) {
                return;
            }
            connection.Close();
        }

        public static bool Start() {
            if (!ConnectDatabase()) return false;
            if (!CreateTables()) return false;
            return true;
            }

        //TODO: add weight to disease
        private static bool CreateTables() {
            try {
                string createQuery;
                createQuery = "CREATE TABLE IF NOT EXISTS DiseaseSymptomsReference " +
                    "(ID INT PRIMARY KEY AUTO_INCREMENT, " +
                    "DiseaseID INT, " +
                    "SymptomID INT," +
                    "FOREIGN KEY (DiseaseID) REFERENCES Disease(ID)," +
                    "FOREIGN KEY (SymptomID) REFERENCES Symptoms(ID)" +
                    ");";
                var reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Disease " +
                    "(ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Description TEXT, " +
                    "Href TEXT, " +
                    "Name TEXT " +
                    ");";
                reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS SymptomRegionsReference (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Symptom INT, " +
                    "Region INT, " +
                    "FOREIGN KEY (Symptom) REFERENCES Symptoms(ID), " +
                    "FOREIGN KEY (Region) REFERENCES Regions(ID)" +
                    ");";
                reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Regions (" +
                    "ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                    "Name TEXT" +
                    ");";
                reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Symptoms (" +
                    "ID INT NOT NULL AUTO_INCREMENT PRIMARY KEY, " +
                    "Region INT, " +
                    "Name TEXT, " +
                    "Description TEXT" +
                    ");";
                reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                createQuery = "CREATE TABLE IF NOT EXISTS Users (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Username VARCHAR(255) NOT NULL, " +
                    "Password VARBINARY(256) NOT NULL, " +
                    "UNIQUE (Username)" +
                    ");";
                reader = new MySqlCommand(createQuery, connection).ExecuteNonQuery();
                return true;
            }
            catch {
                _ = Log.Error(new Exception("Failed to create tables"), "DatabaseController", "");
                return false;
            }

        }

        //Gets all diseases along with the symptoms and regions
        public static async Task<ICollection<IDisease>> GetDiseaseAsync() {
            List<IDisease> diseaseList = new List<IDisease>();
            //Get all diseases
            string query = "SELECT * FROM Disease";
            var reader = new MySqlCommand(query, connection).ExecuteReader();
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

            //Get their symptoms
            List<ISymptom> symptoms = (List<ISymptom>)await GetSymptomsAsync(); //This gets the symptoms regions aswell

            query = "SELECT * FROM DiseaseSymptomsReference";

            Dictionary<int, ISymptom> symptomDic = symptoms.ToDictionary(symptom => symptom.ID);

            Dictionary<int, IDisease> diseaseDic = diseaseList.ToDictionary(disease => disease.ID);

            while(reader.Read()) {
                int diseaseID = reader.GetInt32(1);
                int symptomID = reader.GetInt32(2);

                if(diseaseDic.TryGetValue(diseaseID, out IDisease disease) &&
                    symptomDic.TryGetValue(symptomID, out ISymptom symptom)) {
                    disease.AddSymptoms(symptom);
                }
            }
            return diseaseList;
        }

        //Gets all symptoms alongside its regions
        public static async Task<ICollection<ISymptom>> GetSymptomsAsync() {
            List<ISymptom> symptoms = new List<ISymptom>();
            string query = "SELECT * FROM Symptoms";
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while (reader.Read()) {
                string name = reader.GetString(2);
                int id = reader.GetInt32(0);
                string description = reader.GetString(3);
                ISymptom symptom = new Symptom(name) {
                    Description = description
                    };
                symptom.ID = id;
                symptoms.Add(symptom);
                }


            query = "SELECT * FROM RegionSymptoms";

            List<IRegion> regions = (List<IRegion>)await GetRegionsAsync();

            Dictionary<int, IRegion> regionDic = regions.ToDictionary(region => region.ID);

            Dictionary<int, ISymptom> symptomDic = symptoms.ToDictionary(symptom => symptom.ID);

            while(reader.Read()) {
                int symptomID = reader.GetInt32(1);
                int regionID = reader.GetInt32(2);

                if(symptomDic.TryGetValue(symptomID, out ISymptom symptom) && regionDic.TryGetValue(regionID, out IRegion region)) {
                    symptom.AddRegion(region);
                }
            }
            return symptoms;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds disease to the database
        public static async Task<bool> AddDiseaseAsync(IDisease disease) {
            disease.ID = 1;
            string query = "SELECT ID FROM Disease";
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                disease.ID = reader.GetInt32(0) + 1;
                }

            query = $"INSERT INTO Disease (Description, Href, Name) VALUES ('{disease.Description}', '{disease.Href}', '{disease.Name}');";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            foreach (ISymptom symptom in disease.Symptoms) {
                query = $"INSERT INTO DiseaseSymptomsReference (DiseaseID, SymptomID) VALUES ({disease.ID}, {symptom.ID});";
                new MySqlCommand(query, connection).ExecuteNonQuery();
                }
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds symptom to the database
        public static async Task<bool> AddSymptomAsync(ISymptom symptom) {
            try {
                string query = "SELECT Symptom FROM RegionSymptoms";//Get the reference of regions for the symptom
                int lastRefID = -1;
                bool hasData = false;
                var reader = new MySqlCommand(query, connection).ExecuteReader();
                while(reader.Read()) {
                    hasData = true;
                    lastRefID = reader.GetInt32(0);
                }
                if (!hasData) {//If the database is empty
                    lastRefID= 0;
                    }

                if (lastRefID == -1) {//It should not end in here
                    _=Log.Error(new Exception($"lasRefID was {lastRefID}"), "AddSymptomAsync", "");
                    return false;
                    }
                lastRefID++;//ID always starts at 1
                query = $"INSERT INTO Symptoms (Region, Name, Description) VALUES ('{lastRefID}', '{symptom.Name}', '{symptom.Description}')";
                var insertion = new MySqlCommand(query, connection).ExecuteReaderAsync();
                foreach (IRegion region in symptom.Regions) {
                    await AddSympRegionReferenceAsync(lastRefID, region.ID);
                    }
                if (symptom.Regions.Count == 0) {
                    await AddSympRegionReferenceAsync(lastRefID, 0);
                    }
                await insertion;
                return true;
            }
            catch (Exception ex) {
                _ = Log.Error(ex, "AddSymptomAsync", "");
                return false;
            }
        }

        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            string query = $"INSERT INTO RegionSymptoms (Symptom, Region) VALUES ({sympID}, {regionID})";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Read all of the regions and if any matches, don't add it
        //possibility of wrong IDs comes from this, if any mismatch with regions pops up, check this
        public static async Task<bool> AddRegionAsync(IRegion region) {
            string query = "SELECT Name FROM Regions";
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                if (reader.GetString(0).ToLower() == region.Name.ToLower()) {
                    _ = Log.Warning("Tried to add a Region that already exist", "AddRegionAsync", "");
                    return false;
                }
            }
            query = $"INSERT INTO Regions (Name) VALUES ('{region.Name}');";
            new MySqlCommand(query , connection).ExecuteNonQuery();
            return true;
            }

        //Gets regions
        public static async Task<ICollection<IRegion>> GetRegionsAsync() {
            List<IRegion> regions = new List<IRegion>();
            string query = "SELECT * FROM Regions";
            var reader = new MySqlCommand(query,connection).ExecuteReader();
            while(reader.Read()) {
                regions.Add(new Region(reader.GetString(1), reader.GetInt32(0)));
            }
            return regions;
        }

        public static async Task<bool> ModifyDiseaseAsync(IDisease disease) {
            throw new NotImplementedException();
            return true;
        }

        public static async Task<bool> ModifySymptomAsync(ISymptom symptom) {
            throw new NotImplementedException();
            return true;
        }

        public static async Task<bool> ModifyRegionAsync(IRegion region) {
            string query = $"UPDATE Regions SET Name = '{region.Name}'";
            return true ? false : 0 < new MySqlCommand(query, connection).ExecuteNonQuery();
        }
    }
}
