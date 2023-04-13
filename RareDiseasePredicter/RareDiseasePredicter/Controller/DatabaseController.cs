using Microsoft.Data.Sqlite;
using RareDiseasePredicter.Implementations;
using RareDiseasePredicter.Interfaces;

namespace RareDiseasePredicter.Controller {
    static class DatabaseController {

        private static SqliteConnection Connection;

        private static bool CreateConnection() {
            if (Connection == null) {
                try {
                    Connection = new SqliteConnection("Data Source = Database.sqlite;Version=3;");
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

        private static bool CreateTables() {
            try {
                string createQuery;
                createQuery = "CREATE TABLE DiseaseSymptomsReference (ID int identity(1,1) not null primary key," +
                    "DiseaseID int not null," +
                    "SymptomID int not null);";
                var command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE Disease" +
                    " (ID int identity(1,1) not null primary key," +
                    " Description VARCHAR(MAX)," +
                    " Href VARCHAR(MAX)," +
                    " Name VARCHAR(512)" +
                    " );";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE RegionSymptoms (" +
                    "ID int identity(1,1) not null primary key," +
                    "SymptomRef int not null," +
                    "RegionRef int not null);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                createQuery = "CREATE TABLE Regions (" +
                    " ID int identity(1,1) not null primary key," +
                    " Name VARCHAR(512)," +
                    " SympRef int not null);";
                command = Connection.CreateCommand();
                command.CommandText = createQuery;
                command.ExecuteNonQuery();

                //TODO: Update Region to reference RegionSymptoms
                createQuery = "CREATE TABLE Symptoms (" +
                    " ID int identity(1,1) not null primary key," +
                    " Region int NOT NULL," +
                    " Name VARCHAR(MAX)," +
                    " Description VARCHAR(MAX)," +
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

        public static async Task<ICollection<ISymptom>> GetSymptomsAsync() {
            List<ISymptom> symptomList = new List<ISymptom>();
            List<int> regionIDs = new List<int>();
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
                        symptomList.Add(symptom);
                        regionIDs.Add(reader.GetInt32(1));
                        }
                    }
                command.CommandText = "SELECT * FROM RegionSymptoms";
                List<Tuple<int, IRegion>> regionTuples = (List<Tuple<int, IRegion>>)await GetRegionTuplesAsync();
                using (var reader = command.ExecuteReader()) {
                    int runs = 0;
                    while(reader.Read()) {
                        foreach(ISymptom symptom in symptomList) {
                            if(reader.GetInt32(1) == regionIDs[runs]) {
                                if (reader.GetInt32(2) == regionTuples[runs].Item1) {
                                    symptom.AddRegion(regionTuples[runs].Item2); //This one will be funny to debug if there is an error
                                    }
                                }
                            }
                        runs++;
                        }
                    }
            }
            else {
                _ = Log.Error(new Exception("Could not create Connection"), "GetSymptomsAsync", "");
                return null;
            }
            return symptomList;
        }

        public static async Task<bool> AddDiseaseAsync(IDisease disease) {
            var command = Connection.CreateCommand();
            if(CreateConnection()) {
                foreach(ISymptom symptom in disease.GetSymptoms()) {
                    command.CommandText = $"SELECT * FROM Symptoms WHERE ID = {symptom.ID};";
                    using (var reader = command.ExecuteReader()) {
                        bool found = false;
                        while(await reader.ReadAsync()) {
                            if (symptom.ID == reader.GetInt32(0)) {
                                found = true;
                            }
                        }
                        if(!found) {
                            _ = Log.Error(new Exception($"{disease.Name} : {disease.ID} contains a Symptom ({symptom.Name} : {symptom.ID}) which does not have a maching ID with any other symptoms in the database"), "AddDiseaseAsync", "This method does not automaticly add a new Symptom because expected higher risk of repeated Symptoms");
                        }
                    }
                }
                foreach(ISymptom symptom in disease.GetSymptoms()) {
                    command.CommandText = $"INSERT INTO DiseaseSymptomsReference ({disease.ID}, {symptom.ID});";
                    await command.ExecuteNonQueryAsync();
                }
                command.CommandText = $"INSERT INTO Disease ({disease.ID}, {disease.Description}, {disease.Href}, {disease.Name});";
                await command.ExecuteNonQueryAsync();
            }
            else {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddDiseaseAsync", "");
                return false;
            }
            return true;
        }

        public static async Task<bool> AddSymptomAsync(ISymptom symptom) {
            if (!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddSymptomAsync", "");
                return false;
                }
            try {
                var command = Connection.CreateCommand();
                foreach(IRegion region in symptom.Regions) {
                    if(await AddRegionAsync(region)) {
                        Console.WriteLine($"Added {region.Name} to the database");
                        _ = Log.Warning("Added a Region through AddSymptom", "AddSymptomAsync", "Risk of repeated Region");
                        }
                }
                command.CommandText = "SELECT ID FROM Symptoms";
                using (var reader = command.ExecuteReader()) {
                    if (reader.GetInt32(0) == symptom.ID) {
                        _ = Log.Warning("Tried to add a symptom which already exist", "AddSymptomAsync", "");
                        return false;
                        }
                    }
                var regions = GetRegionsAsync();
                command.CommandText = "SELECT SymptomRef FROM RegionSymptoms";
                int lastRefID = -1;
                using(var reader = command.ExecuteReader()) {
                    while(await reader.ReadAsync()) {
                        lastRefID = reader.GetInt32(0);
                    }
                }
                if (lastRefID == -1 ) {
                    _=Log.Error(new Exception($"lasRefID was {lastRefID}"), "AddSymptomAsync", "");
                    return false;
                    }
                lastRefID++;
                command.CommandText = $"INSERT INTO Symptoms {lastRefID}, {symptom.Name}, {symptom.Description}";
                var query = command.ExecuteNonQueryAsync();
                List<IRegion> _regions = (List<IRegion>)await regions;
                foreach(IRegion region in _regions) {
                    foreach(IRegion sympRegion in symptom.Regions) {
                        if(region.ID == sympRegion.ID) {
                            await AddSympRegionReferenceAsync(lastRefID, region.ID);
                        }
                    }
                }
                await query;
                return true;
            }
            catch (Exception ex) {
                _ = Log.Error(ex, "AddSymptomAsync", "");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Checks if the reference already exist, if not add it
        private static async Task<bool> AddSympRegionReferenceAsync(int sympID, int regionID) {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddSympRegionReferenceAsync", "");
                return false;
                }
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM RegionSymptoms";
            int id0 = -1;
            int id1 = -1;
            using ( var reader = command.ExecuteReader()) {
                while (await reader.ReadAsync()) {
                    id0 = reader.GetInt32(0);
                    id1 = reader.GetInt32(1);
                    if(id0 == sympID && id1 == regionID) {
                        _ = Log.Warning("Tried to add a SympRegion Reference that already exist", "AddSympRegionReferenceAsync", "");
                        return false;
                        }
                    }
            }
            if(id0 == -1 || id1 == -1) {
                _ = Log.Error(new Exception($"id0 had the value of {id0} and id1 had the value of {id1}"), "AddSympRegionReferenceAsync", "Failed in reading RegionSymptoms from database");
                return false;
                }
            command.CommandText = $"INSERT INTO RegionSymptoms {sympID}, {regionID}";
            command.ExecuteNonQuery();
            return true;
        }

        //Read all of the regions and if any matches, don't add it
        public static async Task<bool> AddRegionAsync(IRegion region) {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection to database"), "AddRegionAsync", "");
                return false;
                }
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Name FROM Regions";
            using (var reader = command.ExecuteReader()) {
                while(await reader.ReadAsync()) {
                    if (reader.GetString(0) == region.Name) {
                        _ = Log.Warning("Tried to add a Region that already exist", "AddRegionAsync", "");
                        return false;
                    }
                }

                command.CommandText = "SELECT SymptomRef FROM RegionSymptoms";
                int lastRefID = -1;
                using(var reader_ = command.ExecuteReader()) {
                    while(await reader_.ReadAsync()) {
                        lastRefID = reader_.GetInt32(1);
                    }
                }
                if (lastRefID == -1) {
                    _ = Log.Error(new Exception("lastRefID was -1"), "AddRegionAsync", "");
                    return false;
                    }

                lastRefID++;

                command.CommandText = $"INSERT INTO Regions {region.Name}, {lastRefID};";
                await command.ExecuteNonQueryAsync();
                return true;
            }
        }

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
        public static async Task<ICollection<Tuple<int, IRegion>>> GetRegionTuplesAsync() {
            if(!CreateConnection()) {
                _ = Log.Error(new Exception("Could not create connection"), "GetRegionTuplesAsync", "");
                return null;
                }
            List<Tuple<int, IRegion>> regions = new List<Tuple<int, IRegion>>();
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT * FROM Regions";
            using (var reader = command.ExecuteReader()) {  
                while(await reader.ReadAsync()) {
                    regions.Add(new Tuple<int, IRegion>(reader.GetInt32(2) ,new Region(reader.GetString(1), reader.GetInt32(0))));
                }
            }
            return regions;
        }
    }
}
