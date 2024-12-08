using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection.PortableExecutable;
using System.Formats.Tar;

/**
 * 
 * OWNER: Simon dos Reis Spedsbjerg
 * Date: 15/09/2024
 * Project: RareDiseasePredictor
 * 
 */


namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        private class MisconfiguartionConnectionException : Exception {
            
        }

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
                    isConnected = true;
                    connection.Open();
                    return true;
                }
                catch {
                    isConnected = false;
                    _ = Log.Error(new Exception("Unable to open connection to MYSQL database"), "DatabaseController", "ConnectDatabase");
                    return false;
                }
            }
            else if (isConnected == true) {
                return true;
            }
            throw new MisconfiguartionConnectionException();
        }

        public static void CloseDatabase() {
            if(connection is null) {
                return;
            }
            //isConnected = false;
            connection.Close();
            isConnected = false;
        }

        public static async Task<bool> Start() {
            try {
                if(ConnectDatabase() == false) {
                    CloseDatabase();
                    return false;
                }
                else CloseDatabase();
            }
            catch {
                return false;
            }
            if (await CreateTablesAsync() == false) return false;
            return true;
            }

        //TODO: add weight to disease
        private static async Task<bool> CreateTablesAsync() {
            string createQuery;
            ConnectDatabase();
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
                    "Name varchar(255) NOT NULL, " +
                    "Description TEXT," +
                    "PRIMARY KEY (ID)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create Symptoms table"), "DatabaseController", "");
                return false;
            }

            try {
                createQuery = "USE db; CREATE TABLE IF NOT EXISTS Regions (" +
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
                createQuery = "USE db; CREATE TABLE IF NOT EXISTS DiseaseSymptomsReference " +
                    "(ID INT NOT NULL AUTO_INCREMENT, " +
                    "DiseaseID INT NOT NULL, " +
                    "SymptomID INT NOT NULL," +
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
                createQuery = "USE db; CREATE TABLE IF NOT EXISTS SymptomRegionsReference (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Symptom INT, " +
                    "Region INT " +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create SymptomRegionsReference table"), "DatabaseController", "");
                return false;
            }
            try {
                createQuery = "USE db; CREATE TABLE IF NOT EXISTS Users (" +
                    "ID INT AUTO_INCREMENT PRIMARY KEY, " +
                    "Username VARCHAR(255) NOT NULL, " +
                    "Password VARBINARY(256) NOT NULL, " +
                    "Role INT NOT NULL, " +
                    "UNIQUE (Username)" +
                    ");";
                new MySqlCommand(createQuery, connection).ExecuteNonQuery();
            }
            catch {
                CloseDatabase();
                _ = Log.Error(new Exception("Failed to create Users table"), "DatabaseController", "");
                return false;
            }
            CloseDatabase();
            return true;
        }

        //Gets all diseases along with the symptoms and regions
        public static async Task<ICollection<IDisease>> GetDiseaseAsync() {
            var con = ConnectDatabase();
            List<IDisease> diseaseList = new List<IDisease>();
            //Get all diseases
            string query = "USE db; SELECT * FROM Disease";
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
            reader.Close();

            //Get their symptoms
            List<ISymptom> symptoms = (List<ISymptom>)await GetSymptomsAsync(); //This gets the symptoms regions aswell

            query = "USE db; SELECT * FROM DiseaseSymptomsReference";

            Dictionary<int, ISymptom> symptomDic = symptoms.ToDictionary(symptom => symptom.ID);

            Dictionary<int, IDisease> diseaseDic = diseaseList.ToDictionary(disease => disease.ID);
            reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                int diseaseID = reader.GetInt32(1);
                int symptomID = reader.GetInt32(2);

                if(diseaseDic.TryGetValue(diseaseID, out IDisease disease) &&
                    symptomDic.TryGetValue(symptomID, out ISymptom symptom)) {
                    disease.AddSymptoms(symptom);
                }
            }
            reader.Close();
            return diseaseList;
        }

        //Gets all symptoms alongside its regions
        //FIX ME! Duplicates
        public static async Task<ICollection<ISymptom>> GetSymptomsAsync() {
            ConnectDatabase();
            List<ISymptom> symptoms = new List<ISymptom>();
            string query = "USE db; SELECT * FROM Symptoms";
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
            reader.Close();


            query = "USE db; SELECT * FROM SymptomRegionsReference";

            List<IRegion> regions = (List<IRegion>)await GetRegionsAsync();

            Dictionary<int, IRegion> regionDic = regions.ToDictionary(region => region.ID);

            Dictionary<int, ISymptom> symptomDic = symptoms.ToDictionary(symptom => symptom.ID);
            reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                int symptomID = reader.GetInt32(1);
                int regionID = reader.GetInt32(2);

                if(symptomDic.TryGetValue(symptomID, out ISymptom symptom) && regionDic.TryGetValue(regionID, out IRegion region)) {
                    symptom.AddRegion(region);
                }
            }
            reader.Close();
            return symptoms;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds disease to the database
        public static async Task<bool> AddDiseaseAsync(IDisease disease) {
            disease.ID = 1;
            string query = "USE db; SELECT ID FROM Disease;";
            if(isConnected == false) {
                ConnectDatabase();
            }
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            try {
                while(reader.Read()) {
                    disease.ID = reader.GetInt32(0) + 1;
                }
            }
            catch(AggregateException e) {
                disease.ID = 0;
            }
            reader.Close();
            query = $"USE db; INSERT INTO Disease (Description, Href, Name) VALUES ('{disease.Description}', '{disease.Href}', '{disease.Name}');";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            foreach (ISymptom symptom in disease.Symptoms) {
                await AddSymptomAsync(symptom, disease.ID);
                }
            return true;
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Adds symptom to the database
        //FIX ME! Duplicates
        public static async Task<bool> AddSymptomAsync(ISymptom symptom, int diseaseID) {
            ConnectDatabase();
            try {
                string query = $"USE db; INSERT INTO Symptoms (Name, Description) VALUES ('{symptom.Name}', '{symptom.Description}')";
                new MySqlCommand(query, connection).ExecuteNonQuery();

                query = "SELECT ID FROM Symptoms ORDER BY ID DESC LIMIT 1;";
                var reader = new MySqlCommand(query, connection).ExecuteReader();
                while(reader.Read()) {
                    symptom.ID = (int)reader.GetUInt32(0);
                }
                reader.Close();
                query = $"USE db; INSERT INTO DiseaseSymptomsReference (DiseaseID, SymptomID) VALUES ({diseaseID}, {symptom.ID});";
                new MySqlCommand(query, connection).ExecuteNonQuery();

                foreach(IRegion region in symptom.Regions) {
                    await AddRegionAsync(region, symptom.ID);
                }

                CloseDatabase();
                return true;
            }
            catch (Exception ex) {
                _ = Log.Error(ex, "AddSymptomAsync", "");
                CloseDatabase();
                return false;
            }
        }

        //IMPORTANT: ADMIN TOOL, NOT INTENDED FOR CLIENT USAGE
        //Read all of the regions and if any matches, don't add it
        //FIX ME! Large risk of duplicates, FIX
        public static async Task<bool> AddRegionAsync(IRegion region, int symptomID) {
            ConnectDatabase();
            string query = $"USE db; INSERT INTO Regions (Name) VALUES ('{region.Name}');";
            new MySqlCommand(query, connection).ExecuteNonQuery();

            query = "SELECT ID FROM Regions ORDER BY ID DESC LIMIT 1;";
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            int regID = 0;
            while(reader.Read()) {
                regID = (int)reader.GetUInt32(0);
            }
            reader.Close();
            query = $"USE db; INSERT INTO SymptomRegionsReference (Symptom, Region) VALUES ({symptomID}, {regID});";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            return true;
        }

        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            ConnectDatabase();
            string query = $"USE db; INSERT INTO SymptomRegionsReference (Symptom, Region) VALUES ({sympID}, {regionID})";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            return true;
        }

        private static async Task<bool> RemoveSympRegionReferenceAsync(int sympID, int regionID) {
            ConnectDatabase();
            string query = $"USE db; DELETE FROM SymptomRegionsReference WHERE Symptom = '{sympID}', Region = '{regionID}';";
            await new MySqlCommand(query, connection).ExecuteNonQueryAsync();
            return true;
        }

        private static async Task<bool> AddDiseaseSymptomReferenceAsync(int diseaseID, int symptomID) {
            ConnectDatabase();
            string query = $"USE db; INSERT INTO DiseaseSymptomsReference (DiseaseID, SymptomID) VALUES ({diseaseID}, {symptomID})";
            new MySqlCommand(query, connection).ExecuteNonQuery();
            return true;
        }

        private static async Task<bool> RemoveDiseaseSymptomReferenceAsync(int diseaseID, int symptomID) {
            ConnectDatabase();
            string query = $"USE db; DELETE FROM DiseaseSymptomsReference WHERE DiseaseID = '{diseaseID}', SymptomID = '{symptomID}';";
            await new MySqlCommand(query, connection).ExecuteNonQueryAsync();
            return true;
        }

        //Gets regions
        public static async Task<ICollection<IRegion>> GetRegionsAsync() {
            List<IRegion> regions = new List<IRegion>();
            string query = "USE db; SELECT * FROM Regions";
            var reader = new MySqlCommand(query,connection).ExecuteReader();
            while(reader.Read()) {
                regions.Add(new Region(reader.GetString(1), reader.GetInt32(0)));
            }
            reader.Close();
            return regions;
        }

        public static async Task<bool> ModifyDiseaseAsync(IDisease disease) {
            List<(int, int)> relationIDs = new List<(int, int)>();
            string query = "USE db; SELECT * FROM DiseaseSymptomsReference;";
            ConnectDatabase();
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                relationIDs.Add((reader.GetInt32(1), reader.GetInt32(2)));
            }
            reader.Close();
            List<(int, int)> newRelations = new List<(int, int)>();
            foreach(Symptom symptom in disease.Symptoms) {
                if(relationIDs.Contains((disease.ID, symptom.GetID()))) {
                    newRelations.Add((disease.ID, symptom.GetID()));
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
            query = $"USE db; UPDATE Disease SET Name = '{disease.Name}', Description = '{disease.Description}' WHERE ID = {disease.ID};";
            ConnectDatabase();
            new MySqlCommand(query, connection).ExecuteNonQuery();
            query = $"USE db; SELECT SymptomID FROM DiseaseSymptomsReference WHERE DiseaseID = {disease.ID};";
            reader = new MySqlCommand(query, connection).ExecuteReader();
            int lastRef = 0;
            while(reader.Read()) {
                lastRef = reader.GetInt32(0);
            }
            CloseDatabase();
            foreach((int, int) addition in additionRelations) {
                await AddSympRegionReferenceAsync(lastRef, addition.Item2);
            }
            foreach((int, int) removal in removalRelations) {
                await RemoveSympRegionReferenceAsync(lastRef, removal.Item2);
            }
            return true;
        }

        public static async Task<bool> ModifySymptomAsync(ISymptom symptom) {
            List<(int, int)> relationIDs = new List<(int, int)>();
            string query = "USE db; SELECT * FROM SymptomRegionsReference;";
            ConnectDatabase();
            var reader = new MySqlCommand(query, connection).ExecuteReader();
            while(reader.Read()) {
                relationIDs.Add((reader.GetInt32(0), reader.GetInt32(1)));
            }
            reader.Close();
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
            query = $"USE db; UPDATE Symptoms SET Name = '{symptom.Name}', Description = '{symptom.Description}' WHERE ID = {symptom.ID};";
            ConnectDatabase();
            new MySqlCommand(query, connection).ExecuteNonQuery();
            query = $"USE db; SELECT Region FROM SymptomRegionsReference WHERE Symptom = {symptom.ID};";
            reader = new MySqlCommand(query, connection).ExecuteReader();
            int lastRef = 0;
            while(reader.Read()) {
                lastRef = reader.GetInt32(0);
            }
            reader.Close();
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
            string query = $"USE db; UPDATE Regions SET Name = '{region.Name}' WHERE ID = '{region.ID}';";
            ConnectDatabase();
            new MySqlCommand(query, connection).ExecuteNonQuery();
            CloseDatabase();
            return true;
        }
    }
}
