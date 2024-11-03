using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection.PortableExecutable;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 15/09/2024
 * Project: RareDiseasePredictor
 * 
 */

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {


        private static string Server = "127.0.0.1";
        private static string DatabaseName = "db";
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
            if(isConnected == false) {
                try {
                    string connstring = string.Format("server={0};port={1};database={2};uid={3};password={4}", Server, port, DatabaseName, UserName, Password);
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
            isConnected = false;
            connection.Close();
        }

        public static bool Start() {
            if (!ConnectDatabase()) return false;
            if (!CreateTables()) return false;
            return true;
            }

        //TODO: add weight to disease
        private static bool CreateTables() {
            string createQuery;
            try {
                createQuery = "CREATE TABLE IF NOT EXISTS Disease " +
                    "(ID INT NOT NULL AUTO_INCREMENT, " +
                    "Description TEXT, " +
                    "Href varchar(255), " +
                    "Name varchar(255), " +
                    "PRIMARY KEY (ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create Disease table"), "DatabaseController", "");
                return false;
            }

            try {
                createQuery = "CREATE TABLE IF NOT EXISTS Symptoms (" +
                    "ID INT NOT NULL AUTO_INCREMENT, " +
                    "Region INT, " +
                    "Name varchar(255) NOT NULL, " +
                    "Description TEXT," +
                    "PRIMARY KEY (ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create third table"), "DatabaseController", "");
                return false;
            }

            try {
                createQuery = "CREATE TABLE IF NOT EXISTS Regions (" +
                    "ID INT NOT NULL AUTO_INCREMENT, " +
                    "Name varchar(255) NOT NULL," +
                    "PRIMARY KEY (ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create Regions table"), "DatabaseController", "");
                return false;
            }
            try {
                createQuery = "CREATE TABLE IF NOT EXISTS DiseaseSymptomsReference " +
                    "(ID INT NOT NULL AUTO_INCREMENT, " +
                    "DiseaseID INT NOT NULL, " +
                    "SymptomID INT NOT NULL," +
                    "FOREIGN KEY (DiseaseID) REFERENCES Disease(ID)," +
                    "FOREIGN KEY (SymptomID) REFERENCES Symptoms(ID)," +
                    "PRIMARY KEY (ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create DiseaseSymptomsReference table"), "DatabaseController", "");
                return false;
            }
            try {
                createQuery = "CREATE TABLE IF NOT EXISTS SymptomRegionsReference (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Symptom INT, " +
                    "Region INT, " +
                    "FOREIGN KEY (Symptom) REFERENCES Symptoms(ID), " +
                    "FOREIGN KEY (Region) REFERENCES Regions(ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create third table"), "DatabaseController", "");
                return false;
            }
            try {
                createQuery = "CREATE TABLE IF NOT EXISTS Users (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Username VARCHAR(255) NOT NULL, " +
                    "Password VARBINARY(256) NOT NULL, " +
                    "UNIQUE (Username)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create third table"), "DatabaseController", "");
                return false;
            }
            CloseDatabase();
            return true;
        }

        //Gets all diseases along with the symptoms and regions
        public static async Task<ICollection<IDisease>> GetDiseaseAsync() {
            if(!ConnectDatabase()) return null;
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
            CloseDatabase();
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
            ConnectDatabase();
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
                CloseDatabase();
                return true;
            }
            catch (Exception ex) {
                _ = Log.Error(ex, "AddSymptomAsync", "");
                CloseDatabase();
                return false;
            }
        }

        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            ConnectDatabase();
            string query = $"INSERT INTO SymptomRegionsReference (Symptom, Region) VALUES ({sympID}, {regionID})";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            CloseDatabase();
            return true;
        }

        private static async Task<bool> RemoveSympRegionReferenceAsync(int sympID, int regionID) {
            ConnectDatabase();
            string query = $"DELETE FROM SymptomRegionsReference WHERE Symptom = '{sympID}', Region = '{regionID}';";
            await new MySqlCommand(query, connection).ExecuteNonQueryAsync();
            CloseDatabase();
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Read all of the regions and if any matches, don't add it
        //possibility of wrong IDs comes from this, if any mismatch with regions pops up, check this
        public static async Task<bool> AddRegionAsync(IRegion region) {
            if (!ConnectDatabase()) return false;
            string query = "SELECT Name FROM Regions";
            var command = new MySqlCommand(query, connection);
            var reader = command.ExecuteReader();
            while(reader.Read()) {
                if (reader.GetString(1).ToLower() == region.Name.ToLower()) {
                    _ = Log.Warning("Tried to add a Region that already exist", "AddRegionAsync", "");
                    return false;
                }
            }
            CloseDatabase(); //FOR SOME FUCKING REASON THIS HAS TO BE CLOSED AND THEN OPENED! FIX! in future iteration (:
            ConnectDatabase();
            query = $"INSERT INTO Regions (Name) VALUES ('{region.Name}');";
            new MySqlCommand(query , connection).ExecuteNonQuery();
            CloseDatabase();
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
            List<(int, int)> relationIDs = new List<(int, int)>();
            string query = "SELECT * FROM RegionSymptoms;";
            ConnectDatabase();
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                relationIDs.Add((reader.GetInt32(0), reader.GetInt32(1)));
            }
            CloseDatabase();
            List<(int, int)> newRelations = new List<(int, int)>();
            foreach(Region region in symptom.Regions) {
                if(relationIDs.Contains((symptom.ID, region.ID))) {
                    newRelations.Add((symptom.ID, region.ID));
                }
            }
            List<(int, int)> additionRelations = new List<(int, int)>();
            foreach((int, int) relation in newRelations) {
                if(!relationIDs.Contains(relation)) {
                    additionRelations.Add(relation);
                }
            }
            List<(int, int)> removalRelations = new List<(int, int)>();
            foreach((int, int) relation in newRelations) {
                if(!newRelations.Contains(relation)) {
                    removalRelations.Add(relation);
                }
            }
            query = $"UPDATE Symptoms SET Name = '{symptom.Name}', Description = '{symptom.Description}' WHERE ID = {symptom.ID};";
            ConnectDatabase();
            new MySqlCommand(query, connection).ExecuteNonQuery();
            query = $"SELECT Regions FROM Symptoms WHERE ID = '{symptom.ID};";
            reader = new MySqlCommand(query, connection).ExecuteReader();
            int lastRef = reader.GetInt32(0);
            CloseDatabase();
            foreach((int, int) addition in additionRelations) {
                await AddSympRegionReferenceAsync(lastRef, addition.Item2);
            }
            foreach((int, int) removal in removalRelations) {
                await RemoveSympRegionReferenceAsync(lastRef, removal.Item2);
            }
            return true;
        }

        public static async Task<bool> ModifyRegionAsync(IRegion region) {
            string query = $"UPDATE Regions SET Name = '{region.Name}' WHERE ID = '{region.ID}';";
            ConnectDatabase();
            new MySqlCommand(query, connection).ExecuteNonQuery();
            CloseDatabase();
            return true;
        }
    }
}
